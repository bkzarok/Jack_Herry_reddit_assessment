using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SubRedditStats.Models
{
    public class TokenResponse
    {
        public required string Access_Token { get; set; }

        public int Expires_In { get; set; }

        public required string Token_Type { get; set; }

        public required string Scope { get; set; }
    }
}
