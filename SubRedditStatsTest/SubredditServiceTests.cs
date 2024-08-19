using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using SubRedditStats.Models;
using SubRedditStats.Services;
using Newtonsoft.Json;
using System.Net;
using Moq.Protected;
using System.Threading;
using Microsoft.Extensions.Options;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SubRedditStatsTest
{
    [TestFixture]
    public class SubredditServiceTests: IDisposable
    {
        private Mock<HttpClient> _httpClientMock;
        private Mock<ILogger<SubRedditService>> _loggerMock;
        private Mock<ITokenService> _tokenServiceMock;
        private Mock<IRateLimitingService> _rateLimitingServiceMock;
        private Mock<IOptions<AppSettings>> _appSettingsMock;
        private SubRedditService _subRedditService;
        private AppSettings _appSettings;
        private CancellationTokenSource _cancellationTokenSource;

        [SetUp]
        public void SetUp()
        {
            _httpClientMock = new Mock<HttpClient>();
            _loggerMock = new Mock<ILogger<SubRedditService>>();
            _tokenServiceMock = new Mock<ITokenService>();
            _rateLimitingServiceMock = new Mock<IRateLimitingService>();
            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            _cancellationTokenSource = new CancellationTokenSource();

            _appSettings = new AppSettings { SubRedditUrls = new List<string>{ "https://reddit.com/r/test" } };
            _appSettingsMock.Setup(x => x.Value).Returns(_appSettings);

            _subRedditService = new SubRedditService(_httpClientMock.Object, _tokenServiceMock.Object, _loggerMock.Object, _rateLimitingServiceMock.Object, _appSettingsMock.Object);
        }      

        [Test]
        public void GetSubRedditStats_ShouldReturnNull_WhenSubredditResponseIsNull()
        {
            // Act
            var result = _subRedditService.GetSubRedditStats(null);

            // Assert
            Assert.IsNull(result);
        }

        public void Dispose()
        {           
            _cancellationTokenSource.Dispose();
        }       
    }
}
