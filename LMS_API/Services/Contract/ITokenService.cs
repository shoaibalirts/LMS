using System.Security.Claims;

namespace LMS_API.Services.Contract
{
    public interface ITokenService
    {
        string GenerateToken(int userId, string email, string role);
        DateTime GetTokenExpiryUtc();
        bool TryGetTeacherId(ClaimsPrincipal user, out int teacherId);
        bool TryGetUserId(ClaimsPrincipal user, out int userId);
    }
}