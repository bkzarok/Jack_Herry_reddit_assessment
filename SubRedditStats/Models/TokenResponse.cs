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
