using System.Diagnostics;
using CoralBlog.Models;
using CoralBlog.Repos;
using Microsoft.AspNetCore.Mvc;

namespace CoralBlog.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly RepoPosts _repoPosts;

    public HomeController(ILogger<HomeController> logger, RepoPosts repoPosts)
    {
        _logger = logger;
        _repoPosts = repoPosts;
    }

    public async Task<IActionResult> Index()
    {
        var numberOfPosts = 6;
        var posts = await _repoPosts.GetLastPosts(numberOfPosts);

        if (!posts.Success)
        {
            return StatusCode(500, posts.Error);
        }

        var viewModel = new PostsViewModel { Posts = posts.Return };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
