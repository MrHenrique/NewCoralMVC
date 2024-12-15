using CoralBlog.Models;
using Microsoft.Azure.Cosmos;

namespace CoralBlog.Repos;

public class RepoComments
{
    private readonly CosmosClient _cosmosClient;

    public RepoComments(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task<GeneralReturn<List<Comment>>> GetCommentsByPostId(string postId)
    {
        try
        {
            var container = _cosmosClient.GetContainer("CoralBlogDB", "Comments");
            var queryDefinition = new QueryDefinition(
                "SELECT * FROM c WHERE c.postId = @postId"
            ).WithParameter("@postId", postId);
            var query = container.GetItemQueryIterator<Comment>(queryDefinition);
            var comments = new List<Comment>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                comments.AddRange(response.ToList());
            }

            if (comments.Count == 0)
            {
                Console.WriteLine($"No comments found for PostId: {postId}");
            }

            return new GeneralReturn<List<Comment>>
            {
                Success = true,
                Return = comments,
                Message = "Comments retrieved successfully",
                Error = null
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(
                $"Error retrieving comments for PostId: {postId}, Exception: {e.Message}"
            );
            return new GeneralReturn<List<Comment>>
            {
                Success = false,
                Return = null,
                Message = "Error retrieving comments",
                Error = e.Message
            };
        }
    }

    public async Task<GeneralReturn<Comment>> EditComment(Comment comment, string authorId)
    {
        try
        {
            var containerUser = _cosmosClient.GetContainer("CoralBlogDB", "Users");
            var userResponse = await containerUser.ReadItemAsync<BlogUser>(
                authorId,
                new PartitionKey(authorId)
            );
            comment.Author = userResponse.Resource;
            comment.Date = DateTime.Now;
            var container = _cosmosClient.GetContainer("CoralBlogDB", "Comments");
            var response = await container.UpsertItemAsync(comment);
            return new GeneralReturn<Comment>
            {
                Success = true,
                Return = comment,
                Message = "Comment edited successfully",
                Error = null
            };
        }
        catch (Exception e)
        {
            return new GeneralReturn<Comment>
            {
                Success = false,
                Return = null,
                Message = "Error editing comment",
                Error = e.Message
            };
        }
    }

    public async Task<GeneralReturn<Comment>> CreateComment(Comment comment, string authorId)
    {
        try
        {
            var containerUser = _cosmosClient.GetContainer("CoralBlogDB", "Users");
            var userResponse = await containerUser.ReadItemAsync<BlogUser>(
                authorId,
                new PartitionKey(authorId)
            );
            comment.Author = userResponse.Resource;
            comment.Date = DateTime.Now;
            var container = _cosmosClient.GetContainer("CoralBlogDB", "Comments");
            comment.Id = Guid.NewGuid().ToString();
            var response = await container.CreateItemAsync(comment);
            return new GeneralReturn<Comment>
            {
                Success = true,
                Return = comment,
                Message = "Comment created successfully",
                Error = null
            };
        }
        catch (Exception e)
        {
            return new GeneralReturn<Comment>
            {
                Success = false,
                Return = null,
                Message = "Error creating comment",
                Error = e.Message
            };
        }
    }

    public async Task<GeneralReturn<Comment>> DeleteComment(string id)
    {
        try
        {
            var container = _cosmosClient.GetContainer("CoralBlogDB", "Comments");
            var response = await container.DeleteItemAsync<Comment>(id, new PartitionKey(id));
            return new GeneralReturn<Comment>
            {
                Success = true,
                Return = null,
                Message = "Comment deleted successfully",
                Error = null
            };
        }
        catch (Exception e)
        {
            return new GeneralReturn<Comment>
            {
                Success = false,
                Return = null,
                Message = "Error deleting comment",
                Error = e.Message
            };
        }
    }
}
