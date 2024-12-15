using System;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using CoralBlog.Models;
using Microsoft.Azure.Cosmos;

namespace CoralBlog.Repos
{
    public class RepoUsers
    {
        private readonly CosmosClient _cosmosClient;

        public RepoUsers(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
        }

        public bool VerifyPassword(string hashedPassword, string plainPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
        }

        public string HashPassword(string plainPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainPassword);
        }

        public async Task<GeneralReturn<BlogUser>> GetUserByEmailAndPassword(
            string email,
            string password
        )
        {
            try
            {
                var container = _cosmosClient.GetContainer("CoralBlogDB", "Users");
                var queryDefinition = new QueryDefinition(
                    "SELECT * FROM c WHERE c.email = @Email"
                ).WithParameter("@Email", email);
                var query = container.GetItemQueryIterator<BlogUser>(queryDefinition);
                var user = await query.ReadNextAsync();
                var foundUser = user.FirstOrDefault();

                if (foundUser != null && BCrypt.Net.BCrypt.Verify(password, foundUser.Password))
                {
                    return new GeneralReturn<BlogUser>
                    {
                        Success = true,
                        Return = foundUser,
                        Message = "User retrieved successfully",
                        Error = null
                    };
                }
                else
                {
                    return new GeneralReturn<BlogUser>
                    {
                        Success = false,
                        Return = null,
                        Message = "Invalid email or password",
                        Error = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new GeneralReturn<BlogUser>
                {
                    Success = false,
                    Return = null,
                    Message = "An error occurred",
                    Error = ex.Message
                };
            }
        }

        public async Task<GeneralReturn<BlogUser>> CreateUser(BlogUser user)
        {
            try
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.Role = EnumRole.User;
                user.Id = Guid.NewGuid().ToString();
                var container = _cosmosClient.GetContainer("CoralBlogDB", "Users");
                var response = await container.CreateItemAsync(user);

                return new GeneralReturn<BlogUser>
                {
                    Success = true,
                    Return = response.Resource,
                    Message = "User created successfully",
                    Error = null
                };
            }
            catch (Exception ex)
            {
                return new GeneralReturn<BlogUser>
                {
                    Success = false,
                    Return = null,
                    Message = "An error occurred",
                    Error = ex.Message
                };
            }
        }
    }
}
