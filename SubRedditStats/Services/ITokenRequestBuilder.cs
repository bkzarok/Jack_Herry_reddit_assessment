using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
