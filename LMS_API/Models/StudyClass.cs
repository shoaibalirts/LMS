using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LMS_API.Models
{
    public class StudyClass
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        // ONE Teacher → MANY StudyClasses
        public int TeacherId { get; set; }
        [JsonIgnore]
        public Teacher? Teacher { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // MANY-TO-MANY
        [JsonIgnore]
        public ICollection<StudentStudyClass> StudentStudyClasses { get; set; } = new List<StudentStudyClass>();
    }   
}
