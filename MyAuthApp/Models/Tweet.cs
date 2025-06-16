using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyAuthApp.Models
{
    public class Tweet
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required, MaxLength(280)]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        /* optional – navigation back to User */
        [ForeignKey(nameof(UserId))]
        public User Author { get; set; }
    }
}
