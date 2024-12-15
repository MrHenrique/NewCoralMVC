using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace CoralBlog.Models
{
    public class Post
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        [Required(ErrorMessage = "O campo Título é obrigatório.")]
        public string Title { get; set; }

        [JsonProperty("thumb")]
        [Required(ErrorMessage = "O campo Imagem é obrigatório.")]
        public string Thumb { get; set; }

        [JsonProperty("content")]
        [Required(ErrorMessage = "O campo Conteúdo é obrigatório.")]
        public string Content { get; set; }

        [JsonProperty("author")]
        [Required(ErrorMessage = "O campo Autor é obrigatório.")]
        public BlogUser Author { get; set; }

        [JsonProperty("date")]
        [Required(ErrorMessage = "O campo Data é obrigatório.")]
        public DateTime Date { get; set; }
    }
}
