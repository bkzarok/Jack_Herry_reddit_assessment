using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SubRedditStats.Models;
using SubRedditStats.Services;
namespace SubRedditStats
{

    /// <summary>
    /// This class contains logic to retrieve the authentication Token
    /// </summary>
    public class TokenService : ITokenService
    {
        
        private readonly HttpClient _httpClient;
        private readonly ILogger<TokenService> _logger;
        private readonly ITokenRequestBuilder _tokenRequestBuilder;
        private string? _authToken = null;
        private DateTime _tokenExpiryTime;
        private static readonly object _lock = new object();
        private readonly SemaphoreSlim _semaphoreSlim;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="httpClient"> A <see cref="IHttpClientFactory"/> This to create Http clients</param>
        /// <param name="logger">A <see cref="ILoggerFactory"/> used to write messages</param>
        /// <param name="tokenRequestBuilder"> <see cref="ITokenRequestBuilder"/> use to build the token request</param>
        /// <param name="semaphoreSlim"> <see cref="SemaphoreSlim"/> use to ensure atomic operations on thread sensitive resources</param>
        public TokenService(HttpClient httpClient, ILogger<TokenService> logger, ITokenRequestBuilder tokenRequestBuilder, SemaphoreSlim semaphoreSlim)
        {
            _httpClient = httpClient;
            _logger = logger;
            _tokenRequestBuilder = tokenRequestBuilder;
            _semaphoreSlim = semaphoreSlim;
        }

        public async Task<string> GetAuthTokenAsync(CancellationToken cancellation)
        {
            if (!string.IsNullOrEmpty(_authToken) && DateTime.UtcNow <= _tokenExpiryTime)
                return _authToken;

            await _semaphoreSlim.WaitAsync();

            if (string.IsNullOrEmpty(_authToken) || DateTime.UtcNow >= _tokenExpiryTime)
            {
                _logger.LogInformation("Fetching a new authorization token...");
                var tokenResponse = await FetchTokenAsync(cancellation);

                if (tokenResponse != null)
                {
                    _authToken = tokenResponse.Access_Token;
                    _tokenExpiryTime = DateTime.UtcNow.AddSeconds(tokenResponse.Expires_In);
                    _logger.LogInformation("Authorization token retrieved successfully.");
                }

            }
            _semaphoreSlim?.Release();

            return _authToken;
        }

        private async Task<TokenResponse> FetchTokenAsync(CancellationToken cancellation)
        {
            var tokenRequest = _tokenRequestBuilder.BuildTokenRequest();
            var tokenResponse = await _httpClient.SendAsync(tokenRequest, cancellation);

            if (tokenResponse.IsSuccessStatusCode)
            {
                var content = await tokenResponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(content))
                {
                    return JsonConvert.DeserializeObject<TokenResponse>(content);
                }
                return null;
            }
            else
            {
                _logger.LogError("Failed to retrieve token: {StatusCode}", tokenResponse.StatusCode);
                return null;
            }
        }
    }
}
