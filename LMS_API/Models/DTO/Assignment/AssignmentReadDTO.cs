namespace LMS_API.Models.DTO.Assignment
{
    public class AssignmentReadDTO
    {
        public int Id { get; set; }
        public int Points { get; set; }
        public required string Type { get; set; }
        public required string ClassLevel { get; set; }
        public required string Subject { get; set; }
        public string? PictureUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string? Result { get; set; }
    }
}