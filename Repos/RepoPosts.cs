using CoralBlog.Models;
using Microsoft.Azure.Cosmos;

namespace CoralBlog.Repos;

public class RepoPosts
{
    private readonly CosmosClient _cosmosClient;

    public RepoPosts(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task<GeneralReturn<List<Post>>> GetLastPosts(int numberOfPosts)
    {
        try
        {
            var container = _cosmosClient.GetContainer("CoralBlogDB", "Posts");
            var queryDefinition = new QueryDefinition("SELECT * FROM c ORDER BY c.date DESC");

            if (numberOfPosts > 0)
            {
                queryDefinition = queryDefinition.WithParameter("@numberOfPosts", numberOfPosts);
            }

            var query = container.GetItemQueryIterator<Post>(queryDefinition);
            var posts = new List<Post>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                posts.AddRange(response.ToList());

                if (numberOfPosts > 0 && posts.Count >= numberOfPosts)
                {
                    posts = posts.Take(numberOfPosts).ToList();
                    break;
                }
            }

            return new GeneralReturn<List<Post>>
            {
                Success = true,
                Return = posts,
                Message = "Posts retrieved successfully",
                Error = null
            };
        }
        catch (Exception e)
        {
            return new GeneralReturn<List<Post>>
            {
                Success = false,
                Return = null,
                Message = "Error retrieving posts",
                Error = e.Message
            };
        }
    }

    public async Task<GeneralReturn<Post>> GetPostById(string id)
    {
        try
        {
            var container = _cosmosClient.GetContainer("CoralBlogDB", "Posts");
            var response = await container.ReadItemAsync<Post>(id, new PartitionKey(id));
            return new GeneralReturn<Post>
            {
                Success = true,
                Return = response.Resource,
                Message = "Post retrieved successfully",
                Error = null
            };
        }
        catch (Exception e)
        {
            return new GeneralReturn<Post>
            {
                Success = false,
                Return = null,
                Message = "Error retrieving post",
                Error = e.Message
            };
        }
    }

    public async Task<GeneralReturn<Post>> EditPost(Post post, string userId)
    {
        try
        {
            var containerUser = _cosmosClient.GetContainer("CoralBlogDB", "Users");
            var responseUser = await containerUser.ReadItemAsync<BlogUser>(userId, new PartitionKey(userId));

            post.Author = responseUser.Resource;

            var container = _cosmosClient.GetContainer("CoralBlogDB", "Posts");
            var response = await container.UpsertItemAsync(post);
            return new GeneralReturn<Post>
            {
                Success = true,
                Return = post,
                Message = "Post edited successfully",
                Error = null
            };
        }
        catch (Exception e)
        {
            return new GeneralReturn<Post>
            {
                Success = false,
                Return = null,
                Message = "Error editing post",
                Error = e.Message
            };
        }
    }

    public async Task<GeneralReturn<Post>> CreatePost(Post post)
    {
        try
        {
            var container = _cosmosClient.GetContainer("CoralBlogDB", "Posts");
            post.Id = Guid.NewGuid().ToString();
            var response = await container.CreateItemAsync(post);
            return new GeneralReturn<Post>
            {
                Success = true,
                Return = post,
                Message = "Post created successfully",
                Error = null
            };
        }
        catch (Exception e)
        {
            return new GeneralReturn<Post>
            {
                Success = false,
                Return = null,
                Message = "Error creating post",
                Error = e.Message
            };
        }
    }

    public async Task<GeneralReturn<Post>> DeletePost(string id)
    {
        try
        {
            var container = _cosmosClient.GetContainer("CoralBlogDB", "Posts");
            var response = await container.DeleteItemAsync<Post>(id, new PartitionKey(id));
            return new GeneralReturn<Post>
            {
                Success = true,
                Return = null,
                Message = "Post deleted successfully",
                Error = null
            };
        }
        catch (Exception e)
        {
            return new GeneralReturn<Post>
            {
                Success = false,
                Return = null,
                Message = "Error deleting post",
                Error = e.Message
            };
        }
    }
}
