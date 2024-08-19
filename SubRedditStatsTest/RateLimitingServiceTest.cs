using Microsoft.Extensions.Logging;
using Moq;
using SubRedditStats.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRedditStatsTest
{
    [TestFixture]
    public  class RateLimitingServiceTest: IDisposable
    {
        private Mock<ILogger<RateLimitingService>> _loggerMock;
        private RateLimitingService _rateLimitingService;
        private CancellationTokenSource _cancellationTokenSource;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<RateLimitingService>>();
            _rateLimitingService = new RateLimitingService(_loggerMock.Object);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        [Test]
        public async Task HandleRateLimitingAsync_ShouldNotThrowException_WhenHeadersAreMissing()
        {
            // Arrange
            var response = new HttpResponseMessage();

            // Act & Assert
            Assert.DoesNotThrowAsync(() => _rateLimitingService.HandleRateLimitingAsync(response, _cancellationTokenSource.Token));
        }

        [Test]
        public async Task RateLimitingService_HandleRateLimitingAsync_ShouldSleepsCalculatedTime_WhenValidHeaders()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                Headers = 
                {
                    { "X-Ratelimit-Remaining", "0" },
                    { "X-Ratelimit-Reset", "10" } // Reset after 10 seconds
                }
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            await _rateLimitingService.HandleRateLimitingAsync(response, _cancellationTokenSource.Token);
            stopwatch.Stop();

            // Assert
            Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(10000));
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
