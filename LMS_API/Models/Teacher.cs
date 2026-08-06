using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LMS_API.Models
{
    public class Teacher
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
        
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [JsonIgnore]
        public ICollection<StudyClass> StudyClasses { get; set; } = new List<StudyClass>();

        [JsonIgnore]
        public ICollection<AssignmentSet> AssignmentSets { get; set; } = new List<AssignmentSet>();

        [JsonIgnore]
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        [JsonIgnore]
        public ICollection<AssignedAssignmentSet> AssignedAssignmentSets { get; set; } = new List<AssignedAssignmentSet>();

        [JsonIgnore]
        public ICollection<Student> CreatedStudents { get; set; } = new List<Student>();
    }
}