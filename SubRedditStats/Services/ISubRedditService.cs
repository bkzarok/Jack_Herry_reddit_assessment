using SubRedditStats.Models;

namespace SubRedditStats.Services
{
    /// <summary>
    /// Represents a SubRedditService
    /// </summary>
    public interface ISubRedditService
    {
        /// <summary>
        /// Runs the Service.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RunAsync(int numberOfThreads, CancellationToken cancellationToken);

        /// <summary>
        /// Runs the example.
        /// </summary>
        /// <returns>A SubRadditStats object.</returns>
        SubRadditStats GetSubRedditStats(SubredditResponse subredditResponse);
    }

}
