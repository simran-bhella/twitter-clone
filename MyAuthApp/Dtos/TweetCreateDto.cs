using System.ComponentModel.DataAnnotations;

namespace MyAuthApp.Dtos
{
    public class TweetCreateDto
    {
        [Required, MaxLength(280)]
        public string Content { get; set; }
    }
}
