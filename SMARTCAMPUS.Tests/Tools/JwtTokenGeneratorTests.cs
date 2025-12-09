using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using SMARTCAMPUS.BusinessLayer.Tools;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Tools
{
    public class JwtTokenGeneratorTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly JwtTokenGenerator _jwtTokenGenerator;

        public JwtTokenGeneratorTests()
        {
            _configurationMock = new Mock<IConfiguration>();

            // Setup configuration values
            // _configuration["JwtSettings:Secret"]
            _configurationMock.Setup(x => x["JwtSettings:Secret"]).Returns("SuperSecretKey12345678901234567890");
            _configurationMock.Setup(x => x["JwtSettings:Issuer"]).Returns("TestIssuer");
            _configurationMock.Setup(x => x["JwtSettings:Audience"]).Returns("TestAudience");
            _configurationMock.Setup(x => x["JwtSettings:AccessTokenExpirationMinutes"]).Returns("60");
            _configurationMock.Setup(x => x["JwtSettings:RefreshTokenExpirationDays"]).Returns("7");

            _jwtTokenGenerator = new JwtTokenGenerator(_configurationMock.Object);
        }

        [Fact]
        public void GenerateToken_ShouldReturnValidToken()
        {
            // Arrange
            var user = new User
            {
                Id = "1",
                Email = "test@example.com",
                UserName = "testuser",
                FullName = "Test User"
            };
            var roles = new List<string> { "Admin" };

            // Act
            var tokenDto = _jwtTokenGenerator.GenerateToken(user, roles);

            // Assert
            tokenDto.Should().NotBeNull();
            tokenDto.AccessToken.Should().NotBeNullOrEmpty();
            tokenDto.RefreshToken.Should().NotBeNullOrEmpty();

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenDto.AccessToken);

            jwtToken.Issuer.Should().Be("TestIssuer");
            jwtToken.Audiences.Should().Contain("TestAudience");

            var claims = jwtToken.Claims.ToList();
            claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "1");
            claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "test@example.com");
            claims.Should().Contain(c => c.Type == "FullName" && c.Value == "Test User");

            // Check for role claim.
            // ClaimTypes.Role is "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            // But sometimes it's serialized/deserialized as just "role"
            claims.Should().Contain(c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == "Admin");
        }
    }
}
