using Microsoft.Extensions.Options;
using SubRedditStats.Models;
using System.Text;

namespace SubRedditStats.Services
{
    /// <summary>
    /// This class contains the logic to build the token request;
    /// </summary>
    public class TokenRequestBuilder : ITokenRequestBuilder
    {
        private readonly AppSettings _appSettings;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="appSettings">use to store the url endpoints and
        /// other credentials needed to authorize the client</param>
        public TokenRequestBuilder(IOptions<AppSettings> appSettings) => _appSettings = appSettings.Value;

        public HttpRequestMessage BuildTokenRequest()
        {
            var requestBody = new StringContent(
                $"grant_type={_appSettings.Grant_Type}&username={_appSettings.Username}&password={_appSettings.Password}",
                Encoding.UTF8,
                "application/x-www-form-urlencoded");

            var authenticationString = $"{_appSettings.ClientId}:{_appSettings.Secrets}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));

            var request = new HttpRequestMessage(HttpMethod.Post, _appSettings.TokenUrl)
            {
                Content = requestBody
            };
            request.Headers.Add("Authorization", "Basic " + base64EncodedAuthenticationString);

            return request;
        }
    }

}
