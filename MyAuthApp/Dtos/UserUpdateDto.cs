using System.ComponentModel.DataAnnotations;

namespace MyAuthApp.Dtos
{
    public class UserUpdateDto
    {
        [MaxLength(50)]
        public string Username { get; set; }    // optional; must still be unique if supplied

        [EmailAddress, MaxLength(100)]
        public string Email { get; set; }       // optional; must still be unique if supplied

        [MinLength(6)]
        public string Password { get; set; }    // optional; if supplied, will be re-hashed
    }
}
