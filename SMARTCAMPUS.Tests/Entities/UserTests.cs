using FluentAssertions;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Entities
{
    public class UserTests
    {
        [Fact]
        public void User_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var user = new User
            {
                Id = "1",
                FullName = "John Doe",
                Email = "john.doe@example.com",
                UserName = "johndoe",
                PasswordHash = "hash",
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            // Assert
            user.Id.Should().Be("1");
            user.FullName.Should().Be("John Doe");
            user.Email.Should().Be("john.doe@example.com");
            user.UserName.Should().Be("johndoe");
            user.PasswordHash.Should().Be("hash");
            user.IsActive.Should().BeTrue();
            user.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        }
    }
}
