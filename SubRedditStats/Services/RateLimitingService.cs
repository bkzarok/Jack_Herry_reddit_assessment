using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRedditStats.Services
{
    /// <summary>
    /// This class contains the logic to ensure that the api calls
    /// are space out evenly, in order to ensure maximun throughput 
    /// base ratelimit response parameters
    /// </summary>
    public class RateLimitingService : IRateLimitingService
    {
        private readonly ILogger<RateLimitingService> _logger;
        private int _rateLimitRemaining;
        private int _rateLimitReset;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="logger"> A <see cref="ILoggerFactory"/> used to write messages</param>
        public RateLimitingService(ILogger<RateLimitingService> logger)
        {
            _logger = logger;
        }

        public async Task HandleRateLimitingAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            // Extract rate limit information from headers
            if (response.Headers.Contains("X-Ratelimit-Remaining"))
            {
                _rateLimitRemaining = Convert.ToInt16(Convert.ToDouble(
                    response.Headers.GetValues("X-Ratelimit-Remaining").First()));               
            }

            if (response.Headers.Contains("X-Ratelimit-Reset"))
            {               
                _rateLimitReset = Convert.ToInt16(Convert.ToDouble(
                    response.Headers.GetValues("X-Ratelimit-Reset").First()));              
            }

            if (_rateLimitRemaining == 0)
            {
                _logger.LogWarning("Rate limit reached. Waiting for reset...");
                await Task.Delay(_rateLimitReset * 1000, cancellationToken);  // Wait until reset
            }
            else
            {
                var delay = _rateLimitReset / Math.Max(_rateLimitRemaining, 1) * 1000;
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

}
