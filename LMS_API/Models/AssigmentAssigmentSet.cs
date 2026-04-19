using System.Text.Json.Serialization;

namespace LMS_API.Models
{
    // Just join table for MANY-TO-MANY relation between Assignment and AssignmentSet
    public class AssignmentAssignmentSet
    {
        public int AssignmentId { get; set; }
        [JsonIgnore]
        public Assignment? Assignment { get; set; }

        public int AssignmentSetId { get; set; }
        [JsonIgnore]
        public AssignmentSet? AssignmentSet { get; set; }
    }
}