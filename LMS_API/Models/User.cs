using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string PasswordHash { get; set; }
        [Required]
        public required string Role { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
