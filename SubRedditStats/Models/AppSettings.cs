﻿namespace SubRedditStats.Models
{
    public class AppSettings
    {
        public string ClientId { get; set; }
        public string Secrets { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Grant_Type { get; set; }
        public string TokenUrl { get; set; }
        public List<string> SubRedditUrls { get; set; }
        public string UserAgent { get; set; }

    }
}
