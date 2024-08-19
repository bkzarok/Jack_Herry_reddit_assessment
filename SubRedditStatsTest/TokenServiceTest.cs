using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using SubRedditStats;
using SubRedditStats.Models;
using SubRedditStats.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SubRedditStatsTest
{
    [TestFixture]
    public class TokenServiceTest: IDisposable
    {

        private  Mock<HttpClient> _httpClientMock;
        private  Mock<ILogger<TokenService>> _loggerMock;
        private  Mock<ITokenRequestBuilder> _tokenRequestBuilderMock;
        private  TokenService _tokenService;
        private  SemaphoreSlim _semaphore;

        [SetUp]
        public void SetUp()
        {
            _httpClientMock = new Mock<HttpClient>();
            _loggerMock = new Mock<ILogger<TokenService>>();
            _tokenRequestBuilderMock = new Mock<ITokenRequestBuilder>();
            _semaphore = new SemaphoreSlim(1, 1);

            _tokenService = new TokenService(_httpClientMock.Object, _loggerMock.Object, _tokenRequestBuilderMock.Object, _semaphore);
        }

        [Test]
        public async Task TokenService_GetAuthTokenAsync_ReturnOldToken_WhenTokenIsValid()
        {
            // Arrange
            var expectedToken = "validToken";
            _tokenService.GetType().GetField("_authToken", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_tokenService, expectedToken);
            _tokenService.GetType().GetField("_tokenExpiryTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_tokenService, DateTime.UtcNow.AddMinutes(5));

            // Act
            var token = await _tokenService.GetAuthTokenAsync();

            // Assert
            Assert.That(token, Is.EqualTo(expectedToken));
        }

        public async Task TokenService_GetAuthTokenAsync_ReturnNewToken_WhenTokenIsNull()
        {            
            //act
            var token = await _tokenService.GetAuthTokenAsync();

            //Assert
            Assert.NotNull(token);
        }
       
        public void Dispose()
        {
            _semaphore.Dispose();         
        }
    }
}
