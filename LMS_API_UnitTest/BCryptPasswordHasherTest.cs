using LMS_API.Services;

namespace LMS_API_UnitTest
{
    public class BCryptPasswordHasherTest
    {
        private readonly BCryptPasswordHasher _hasher = new();

        [Fact]
        public void Hash_ReturnsNonEmptyString()
        {
            var hash = _hasher.Hash("mypassword");

            Assert.False(string.IsNullOrEmpty(hash));
        }

        [Fact]
        public void Hash_DoesNotReturnPlainText()
        {
            var hash = _hasher.Hash("mypassword");

            Assert.NotEqual("mypassword", hash);
        }

        [Fact]
        public void Hash_SamePasswordTwice_ProducesDifferentHashes()
        {
            var hash1 = _hasher.Hash("mypassword");
            var hash2 = _hasher.Hash("mypassword");

            Assert.NotEqual(hash1, hash2);
        }

        // InlineData: run the same verify logic against several different passwords
        [Theory]
        [InlineData("mypassword")]
        [InlineData("P@ssw0rd!")]
        [InlineData("123456")]
        public void Verify_CorrectPassword_ReturnsTrue(string password)
        {
            var hash = _hasher.Hash(password);

            Assert.True(_hasher.Verify(password, hash));
        }

        // InlineData: wrong attempt, empty string, and whitespace all fail
        [Theory]
        [InlineData("wrongpassword")]
        [InlineData("")]
        [InlineData("   ")]
        public void Verify_InvalidAttempt_ReturnsFalse(string wrongAttempt)
        {
            var hash = _hasher.Hash("mypassword");

            Assert.False(_hasher.Verify(wrongAttempt, hash));
        }
    }
}
