using LMS_API.Models.DTO.Assignment;

namespace LMS_API.Models.DTO.AssignmentSet
{
    public class AssignmentSetReadDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<AssignmentReadDTO> Assignments { get; set; } = new();
    }
}