using System.Text.Json.Serialization;

namespace SubRedditStats.Models
{
    public class SubredditResponse
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        public string After { get; set; }
        [JsonPropertyName("chidren")]
        public List<Post> Children { get; set; }
    }

    public class Post
    {
        [JsonPropertyName("data")]
        public PostData Data { get; set; }
    }

    public class PostData
    {
        [JsonPropertyName("author_fullname")]
        public string Author_Fullname { get; set; }
        [JsonPropertyName("ups")]
        public int Ups { get; set; }
        public int Downs { get; set; }
        public int Pwls { get; set; }
        public double Upvote_Ratio { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
}
