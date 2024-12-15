namespace CoralBlog.Models;

public class GeneralReturn<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Return { get; set; }
    public string Error { get; set; }
}
