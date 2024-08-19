namespace SubRedditStats.Services
{
    /// <summary>
    /// Represents a TokenRequestBuilder
    /// </summary>
    public interface ITokenRequestBuilder
    {
        /// <summary>
        /// Build the request
        /// </summary>
        /// <returns>An HttpRequestMessage object.</returns>
        HttpRequestMessage BuildTokenRequest();
    }

}
