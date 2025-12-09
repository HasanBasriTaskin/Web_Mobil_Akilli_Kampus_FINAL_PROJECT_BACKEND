using FluentAssertions;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Entities
{
    public class EmailVerificationTokenTests
    {
        [Fact]
        public void EmailVerificationToken_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var token = new EmailVerificationToken
            {
                Id = 1,
                Token = "email_token",
                ExpiresAt = DateTime.Now.AddHours(24),
                UserId = "8",
                User = new User { UserName = "user3" },
                IsVerified = false,
                VerifiedAt = null
            };

            // Assert
            token.Id.Should().Be(1);
            token.Token.Should().Be("email_token");
            token.ExpiresAt.Should().BeAfter(DateTime.Now);
            token.UserId.Should().Be("8");
            token.User.Should().NotBeNull();
            token.IsVerified.Should().BeFalse();
            token.VerifiedAt.Should().BeNull();
        }
    }
}
