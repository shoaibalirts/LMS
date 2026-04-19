using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LMS_API.Models
{
    public class AssignmentSet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }

        // One-to-many (Teacher → AssignmentSets)
        [Required]
        public int TeacherId { get; set; }
        [JsonIgnore]
        public Teacher? Teacher { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // MANY-TO-MANY
        [JsonIgnore]
        public ICollection<AssignmentAssignmentSet> AssignmentAssignmentSets { get; set; } = new List<AssignmentAssignmentSet>();
    }
}