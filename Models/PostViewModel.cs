namespace CoralBlog.Models
{
    public class PostViewModel
    {
        public Post Post { get; set; }
        public List<Comment> Comments { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }
}
