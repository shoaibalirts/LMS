namespace LMS_API.Models.DTO.Auth
{

    // object to return to the client after successful authentication, containing the JWT token and user info
    public class AuthResponseDTO
    {
        public required string Token { get; set; }
        public required string Role { get; set; }
        public required string Email { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }
}