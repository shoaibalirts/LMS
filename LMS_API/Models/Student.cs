using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LMS_API.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }

        public DateTime? CreatedDate { get; set; } // optional. it can be nullable
        public DateTime? UpdatedDate { get; set; } // optional. it can be nullable

        public int? CreatedByTeacherId { get; set; }
        [JsonIgnore]
        public Teacher? CreatedByTeacher { get; set; }

        [JsonIgnore]
        public ICollection<StudentStudyClass> StudentStudyClasses { get; set; } = new List<StudentStudyClass>();
    }
}
