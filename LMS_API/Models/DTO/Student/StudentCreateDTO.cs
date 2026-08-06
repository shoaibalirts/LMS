using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models.DTO.Student
{
    // object for when a teacher register a student, containing all necessary info including password
    public class StudentCreateDTO
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public required string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public required string Password { get; set; }
    }
}
