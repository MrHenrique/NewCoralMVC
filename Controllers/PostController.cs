using System.Diagnostics;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs;
using CoralBlog.Models;
using CoralBlog.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoralBlog.Controllers;

[Route("Post")]
public class PostController : Controller
{
    private readonly ILogger<PostController> _logger;
    private readonly RepoPosts _repoPosts;
    private readonly RepoComments _repoComments;
    private readonly string _blobConnectionString = "YourAzureBlobConnectionString";
    private readonly string _blobContainerName = "YourBlobContainerName";
    private readonly BlobContainerClient _blobClient;

    public PostController(
        ILogger<PostController> logger,
        RepoPosts repoPosts,
        RepoComments repoComments,
        BlobContainerClient blobClient
    )
    {
        _logger = logger;
        _repoPosts = repoPosts;
        _repoComments = repoComments;
        _blobClient = blobClient;
    }

    [HttpGet("{postId}")]
    public async Task<IActionResult> Index(string postId, int? page)
    {
        var post = await _repoPosts.GetPostById(postId);
        var comments = await _repoComments.GetCommentsByPostId(postId);

        if (!post.Success)
        {
            return StatusCode(500, post.Error);
        }

        if (!comments.Success)
        {
            return StatusCode(500, comments.Error);
        }

        int pageSize = 3;
        int pageNumber = page ?? 1;
        int totalComments = comments.Return?.Count ?? 0;
        int totalPages = (int)Math.Ceiling((double)totalComments / pageSize);
        var paginatedComments = comments
            .Return?.Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var viewModel = new PostViewModel
        {
            Post = post.Return,
            Comments = paginatedComments,
            Page = pageNumber,
            TotalPages = totalPages
        };

        return View(viewModel);
    }
[Authorize]
    [HttpPost("CreateComment")]
    public async Task<IActionResult> CreateComment(
        [FromForm] Comment comment,
        [FromForm] string authorId
    )
    {
        var result = await _repoComments.CreateComment(comment, authorId);
        if (result.Success)
        {
            return RedirectToAction("Index", new { postId = comment.PostId });
        }
        return StatusCode(500, result.Error);
    }
[Authorize]
    [HttpPost("EditComment")]
    public async Task<IActionResult> EditComment(
        [FromForm] Comment comment,
        [FromForm] string authorId
    )
    {
        var result = await _repoComments.EditComment(comment, authorId);
        if (result.Success)
        {
            return RedirectToAction("Index", new { postId = comment.PostId });
        }
        return StatusCode(500, result.Error);
    }
[Authorize]
    [HttpPost("DeleteComment")]
    public async Task<IActionResult> DeleteComment([FromForm] string id, [FromForm] string postId)
    {
        var result = await _repoComments.DeleteComment(id);
        if (result.Success)
        {
            return RedirectToAction("Index", new { postId });
        }
        return StatusCode(500, result.Error);
    }
[Authorize]
    [HttpPost("EditPost")]
    public async Task<IActionResult> EditPost([FromForm] Post post, IFormFile Thumb, string userId)
    {
        var blobUrl = await UploadImageToBlob(Thumb, post.Id);
        if (!blobUrl.Success)
        {
            return StatusCode(500, blobUrl.Error);
        }
        post.Thumb = blobUrl.Return;
        var result = await _repoPosts.EditPost(post, userId);
        if (result.Success)
        {
            return RedirectToAction("Index", new { postId = post.Id });
        }
        return StatusCode(500, result.Error);
    }
[Authorize]
    [HttpPost("DeletePost")]
    public async Task<IActionResult> DeletePost([FromForm] string id)
    {
        var result = await _repoPosts.DeletePost(id);
        if (result.Success)
        {
            return RedirectToAction("Index", "Home");
        }
        return StatusCode(500, result.Error);
    }

    private async Task<GeneralReturn<string>> UploadImageToBlob(IFormFile file, string postId)
    {
        try
        {
            var fileName = $"post-{postId}-thumb{Path.GetExtension(file.FileName)}";
            var blobClient = _blobClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(file.OpenReadStream(), true);

            var sasToken = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddYears(50));

            return new GeneralReturn<string> { Success = true, Return = sasToken.ToString() };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new GeneralReturn<string>
            {
                Success = false,
                Return = null,
                Error = e.Message
            };
        }
    }
    [Authorize]
    [HttpGet("New")]
    public IActionResult New()
    {
        return View(new Post());
    }
    [Authorize]
    [HttpPost("CreatePost")]
    public async Task<IActionResult> CreatePost([FromForm] Post post, IFormFile Thumb, string userId)
    {
        var blobUrl = await UploadImageToBlob(Thumb, post.Id);
        if (!blobUrl.Success)
        {
            return StatusCode(500, blobUrl.Error);
        }
        post.Thumb = blobUrl.Return;
        post.Date = DateTime.Now;
        var result = await _repoPosts.CreatePost(post);
        if (result.Success)
        {
            return RedirectToAction("Index", new { postId = post.Id });
        }
        return StatusCode(500, result.Error);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
