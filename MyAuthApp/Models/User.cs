using System;
using System.ComponentModel.DataAnnotations;

namespace MyAuthApp.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }                // Primary key; use a GUID
        
        [Required, MaxLength(50)]
        public string Username { get; set; }        // Unique username
        
        [Required, MaxLength(100)]
        public string Email { get; set; }           // Unique email
        
        [Required]
        public string PasswordHash { get; set; }    // Hashed password via ASP.NET Identity
        
        [Required]
        public DateTime CreatedAt { get; set; }     // Timestamp to know when the user was created
    }
}
