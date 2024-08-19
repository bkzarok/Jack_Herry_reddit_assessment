using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SubRedditStats.Models;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;


namespace SubRedditStats.Services
{
    public class SubRedditService : ISubRedditService
    {
        /// <summary>
        /// This class contains the logic to call multiple request based of the number
        /// of thread available and the number of subreddit urls specified in the
        /// appsettings.json file.   
        /// </summary>
        private readonly HttpClient _httpClient;
        private readonly ILogger<SubRedditService> _logger;
        private readonly ITokenService _tokenService;
        private readonly IRateLimitingService _rateLimitingService;
        private readonly AppSettings _appSettings;
        private readonly Timer _loggingTimer;
        private ConcurrentDictionary<string, int> _contentCount = new();
        private  ConcurrentDictionary<string, string> _urlPaginationMap = new();
        private Post _post = new Post();
        private PostData postData = new PostData { Ups = 0 };
        private string _afterToken=null;


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger{TCategoryName}" /> used to write messages.</param>
        /// <param name="tokenService">An <see cref="ITokenService" /> used to to retrieve the authentication token.</param>
        /// <param name="httpClient">A <see cref="IHttpClientFactory" /> used to create HTTP clients.</param>
        /// <param name="appSettings"> use to store the url and also the other credentials needed to authorize the client"/></param>
        /// <param name="rateLimitingService"> A <see cref="IRateLimitingService"/> Use to control the rate of api 
        /// class using ratelimit response headers</param>
        public SubRedditService(HttpClient httpClient,
            ITokenService tokenService,
            ILogger<SubRedditService> logger,
            IRateLimitingService rateLimitingService,
            IOptions<AppSettings> appSettings)
        {
            _httpClient = httpClient;
            _logger = logger;
            _tokenService = tokenService;
            _rateLimitingService = rateLimitingService;
            _appSettings = appSettings.Value;
            _loggingTimer = new Timer(LogContentPeriodically, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            _post.Data = postData;
        }

        /// <summary>
        /// This method will spin up threads based on minimum number of threads available
        /// and the number of Subredditurls specified in the appsettings.json file. This 
        /// will ensure that maximun utilization of resources.
        /// </summary>
        /// <param name="numberOfThreads"> number of thread available</param>
        /// <param name="cancellationToken"> cancellationToken</param>
        /// <returns></returns>
        public async Task RunAsync(int numberOfThreads, CancellationToken cancellationToken)
        {
            var numberOfUrl = _appSettings.SubRedditUrls.Count;
            numberOfThreads = Math.Min(numberOfThreads, numberOfUrl);
            var tasks = Enumerable.Range(0, numberOfThreads)
                                  .Select(i => Task.Run(() => ProcessRequestsAsync(_appSettings.SubRedditUrls[i],cancellationToken), cancellationToken))
                                  .ToArray();

            await Task.WhenAll(tasks);
        }

        public async Task ProcessRequestsAsync(string url,CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var token = await _tokenService.GetAuthTokenAsync(cancellationToken);
                   // _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    if (!_urlPaginationMap.TryGetValue(url, out var after)) { 
                        _urlPaginationMap.TryAdd(url, "");
                    }
                    var requestUrl = String.IsNullOrEmpty(after) ? url : $"{url}?after={after}";

                    var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await _httpClient.SendAsync(request, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        var subredditResponse = JsonConvert.DeserializeObject<SubredditResponse>(content);
                        if(subredditResponse?.Data?.Children.Count == 0) continue;

                        _urlPaginationMap[url] = subredditResponse?.Data?.After;

                        var subRedditStats = GetSubRedditStats(subredditResponse);

                        if (_contentCount.TryGetValue(subRedditStats.Author, out int value))
                        {
                            _contentCount[subRedditStats.Author] = value + subRedditStats.PostCount;
                        }
                        else
                        {
                            _contentCount.TryAdd(subRedditStats.Author, subRedditStats.PostCount);
                        }

                        _post = (subRedditStats.Post.Data.Ups > _post.Data.Ups) ? subRedditStats.Post : _post;

                        await _rateLimitingService.HandleRateLimitingAsync(response, cancellationToken);
                    }
                    else
                    {
                        _logger.LogError("Request failed with status code: {StatusCode}", response.StatusCode);
                        await Task.Delay(10000, cancellationToken); // Wait before retrying
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during request processing.");
                }
            }
        }

        private void LogContentPeriodically(object state)
        {
            if (_contentCount.Count != 0)
            {
                Console.Clear();
                var keyOfMaxValue = _contentCount.MaxBy(entry => entry.Value);
                Console.WriteLine($"\n\n***********Periodic Report*************\n\nPost with the most up votes:\n{_post.Data.Title}\nTotal Up Votes:{_post.Data.Ups}" +
                    $"\nDowns:{_post.Data.Downs}\nPwls:{_post.Data.Pwls}\nUpvote_Ratio:{_post.Data.Upvote_Ratio}\n"+
                    $"\n\nUser with the most post:{keyOfMaxValue.Key}\nTotal Posts:{keyOfMaxValue.Value}");
            }
        }

        public SubRadditStats GetSubRedditStats(SubredditResponse subredditResponse)
        {
            if (subredditResponse?.Data.Children == null)
            {
                return null;
            }

            // Get the count of unique author_fullname values
            var authorPostCounts = subredditResponse.Data.Children
                .GroupBy(post => post.Data.Author)
                .Select(group => new SubRadditStats
                {
                    Author = group.Key,
                    PostCount = group.Count(),
                    Post = new Post()
                })
                .OrderByDescending(x => x.PostCount)
                .FirstOrDefault();

           var post = subredditResponse.Data.Children
               .OrderByDescending(post => post.Data?.Ups)
               .FirstOrDefault();

            if (authorPostCounts != null )
            {
                authorPostCounts.Post = post;
            }
           
            return authorPostCounts;
        }
    }
}
