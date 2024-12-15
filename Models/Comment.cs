using System;
using Newtonsoft.Json;

namespace CoralBlog.Models
{
    public class Comment
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("postId")]
        public string PostId { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("author")]
        public BlogUser Author { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }
    }
}
