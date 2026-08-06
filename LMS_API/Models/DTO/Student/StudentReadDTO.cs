
namespace LMS_API.Models.DTO.Student
{
    // object for returning student info to the client, excluding sensitive data like password
    // used in studyclassReadDTO to return info about all students in a class
    public class StudentReadDTO
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }
    }
}
