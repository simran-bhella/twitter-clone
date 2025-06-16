using System.ComponentModel.DataAnnotations;

namespace MyAuthApp.Dtos
{
    public class TweetUpdateDto
    {
        [Required, MaxLength(280)]
        public string Content { get; set; }
    }
}
