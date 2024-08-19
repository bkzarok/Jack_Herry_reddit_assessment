namespace SubRedditStats.Services
{
    /// <summary>
    /// Represents a TokenService
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Gets the authentication Token asyncronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<string> GetAuthTokenAsync(CancellationToken cancellationToken);
    }

}
