using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SubRedditStats.Models;
using SubRedditStats.Services;

namespace SubRedditStatsTest
{
    [TestFixture]
    public class SubredditServiceTests
    {
        private Mock<HttpClient> _httpClientMock;
        private Mock<ILogger<SubRedditService>> _loggerMock;
        private Mock<ITokenService> _tokenServiceMock;
        private Mock<IRateLimitingService> _rateLimitingServiceMock;
        private Mock<IOptions<AppSettings>> _appSettingsMock;
        private SubRedditService _subRedditService;
        private AppSettings _appSettings;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void SetUp()
        {
            _httpClientMock = new Mock<HttpClient>();
            _loggerMock = new Mock<ILogger<SubRedditService>>();
            _tokenServiceMock = new Mock<ITokenService>();
            _rateLimitingServiceMock = new Mock<IRateLimitingService>();
            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            _cancellationToken = new CancellationToken();

            _appSettings = new AppSettings { SubRedditUrls = new List<string> { "https://reddit.com/r/test" } };
            _appSettingsMock.Setup(x => x.Value).Returns(_appSettings);

            _subRedditService = new SubRedditService(_httpClientMock.Object, _tokenServiceMock.Object, _loggerMock.Object, _rateLimitingServiceMock.Object, _appSettingsMock.Object);
        }

        [Test]
        public void SubRedditService_GetSubRedditStats_ShouldReturnNull_WhenSubredditResponseIsNull()
        {
            // Act
            var result = _subRedditService.GetSubRedditStats(null);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void SubRedditService_GetSubRedditStats_ShouldReturnCorrectStats_WhenValidSubRedditResponse()
        {
            // Arrange
            var subredditResponse = new SubredditResponse
            {
                Data = new Data
                {
                    Children = new List<Post>
                    {
                        new Post { Data = new PostData { Author = "author1", Ups = 100, Title = "Post1" } },
                        new Post { Data = new PostData { Author = "author1", Ups = 200, Title = "Post2" } },
                        new Post { Data = new PostData { Author = "author2", Ups = 50, Title = "Post3" } }
                    }
                }
            };

            // Act
            var result = _subRedditService.GetSubRedditStats(subredditResponse);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Author, Is.EqualTo("author1"));
            Assert.That(result.PostCount, Is.EqualTo(2));
            Assert.That(result.Post.Data.Title, Is.EqualTo("Post2"));
            Assert.That(result.Post.Data.Ups, Is.EqualTo(200));
        }
    }
}
