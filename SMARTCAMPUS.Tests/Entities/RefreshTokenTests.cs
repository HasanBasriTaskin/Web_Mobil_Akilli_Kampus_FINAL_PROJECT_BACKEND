using FluentAssertions;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Entities
{
    public class RefreshTokenTests
    {
        [Fact]
        public void RefreshToken_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var refreshToken = new RefreshToken
            {
                Id = 1,
                Token = "sample_token",
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = "10",
                User = new User { UserName = "user1" },
                Revoked = null,
                CreatedByIp = "127.0.0.1",
                RevokedByIp = null,
                ReasonRevoked = null
            };

            // Assert
            refreshToken.Id.Should().Be(1);
            refreshToken.Token.Should().Be("sample_token");
            refreshToken.Expires.Should().BeAfter(DateTime.UtcNow);
            refreshToken.IsExpired.Should().BeFalse();
            refreshToken.UserId.Should().Be("10");
            refreshToken.User.Should().NotBeNull();
            refreshToken.IsValid.Should().BeTrue();
            refreshToken.CreatedByIp.Should().Be("127.0.0.1");
        }

        [Fact]
        public void RefreshToken_ShouldBeInvalid_WhenRevoked()
        {
            var refreshToken = new RefreshToken
            {
                Revoked = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(1)
            };
            refreshToken.IsValid.Should().BeFalse();
        }

        [Fact]
        public void RefreshToken_ShouldBeExpired_WhenExpiresIsInPast()
        {
             var refreshToken = new RefreshToken
             {
                 Revoked = null,
                 Expires = DateTime.UtcNow.AddDays(-1)
             };
             refreshToken.IsExpired.Should().BeTrue();
             refreshToken.IsValid.Should().BeFalse();
        }
    }
}
