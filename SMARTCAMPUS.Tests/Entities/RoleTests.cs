using FluentAssertions;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Entities
{
    public class RoleTests
    {
        [Fact]
        public void Role_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var role = new Role
            {
                Id = "1",
                Name = "Admin",
                Description = "Administrator Role"
            };

            // Assert
            role.Id.Should().Be("1");
            role.Name.Should().Be("Admin");
            role.Description.Should().Be("Administrator Role");
        }
    }
}
