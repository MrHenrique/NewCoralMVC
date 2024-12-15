using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace CoralBlog.Models
{
    public class BlogUser
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        [Required(ErrorMessage = "O campo Nome de Usuário é obrigatório.")]
        public string Username { get; set; }

        [JsonProperty("password")]
        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [JsonProperty("email")]
        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O campo Email não é um endereço de email válido.")]
        public string Email { get; set; }

        [JsonProperty("role")]
        public EnumRole Role { get; set; }
    }

    public enum EnumRole
    {
        Admin = 1,
        User = 2
    }
}
