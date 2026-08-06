using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models.DTO.Student
{
    // object for logging in as a student, containing only the email and password
    public class StudentLoginDTO
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public required string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public required string Password { get; set; }
    }
}
