using LMS_API.Services.Contract;

namespace LMS_API.Services
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string plainText) => BCrypt.Net.BCrypt.HashPassword(plainText);
        public bool Verify(string plainText, string hash) => BCrypt.Net.BCrypt.Verify(plainText, hash);
    }
}
