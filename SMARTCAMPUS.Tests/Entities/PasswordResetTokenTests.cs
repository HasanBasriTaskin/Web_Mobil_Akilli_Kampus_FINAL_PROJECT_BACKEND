using FluentAssertions;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Entities
{
    public class PasswordResetTokenTests
    {
        [Fact]
        public void PasswordResetToken_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var token = new PasswordResetToken
            {
                Id = 1,
                Token = "reset_token",
                ExpiresAt = DateTime.Now.AddHours(1),
                UserId = "5",
                User = new User { UserName = "user2" },
                IsUsed = false
            };

            // Assert
            token.Id.Should().Be(1);
            token.Token.Should().Be("reset_token");
            token.ExpiresAt.Should().BeAfter(DateTime.Now);
            token.UserId.Should().Be("5");
            token.User.Should().NotBeNull();
            token.IsUsed.Should().BeFalse();
        }
    }
}
