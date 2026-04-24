using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LMS_API.Services.Contract;
using Microsoft.IdentityModel.Tokens;

namespace LMS_API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(int userId, string email, string role)
        {
            var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
            var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
            var audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing.");
            var expiryUtc = GetTokenExpiryUtc();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Role, role),
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiryUtc,
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public DateTime GetTokenExpiryUtc()
        {
            var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var parsed)
                ? parsed
                : 120;

            return DateTime.UtcNow.AddMinutes(expiryMinutes);
        }

        public bool TryGetTeacherId(ClaimsPrincipal user, out int teacherId)
        {
            return TryGetUserId(user, out teacherId);
        }

        public bool TryGetUserId(ClaimsPrincipal user, out int userId)
        {
            userId = default;
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out userId);
        }
    }
}