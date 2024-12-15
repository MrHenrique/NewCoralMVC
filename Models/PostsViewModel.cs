namespace CoralBlog.Models;

public class PostsViewModel
{
    public List<Post> Posts { get; set; } = new List<Post>();
    public int? Page { get; set; }
}
