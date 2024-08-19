using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRedditStats.Services
{
    /// <summary>
    /// Represents a ReateLimitingService
    /// </summary>
    public interface IRateLimitingService
    {
        /// <summary>
        /// Handles Rate Limiting
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandleRateLimitingAsync(HttpResponseMessage response, CancellationToken cancellationToken);
    }
}
