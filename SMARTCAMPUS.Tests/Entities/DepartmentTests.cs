using FluentAssertions;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Entities
{
    public class DepartmentTests
    {
        [Fact]
        public void Department_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var department = new Department
            {
                Id = 1,
                Name = "Computer Science",
                Code = "CS",
                FacultyName = "Engineering",
                Description = "CS Dept",
                IsActive = true,
                CreatedDate = DateTime.Now,
                // Audit fields not in model or inherited properly? Let's check BaseEntity or skip if not present
            };

            // Assert
            department.Id.Should().Be(1);
            department.Name.Should().Be("Computer Science");
            department.Code.Should().Be("CS");
            department.FacultyName.Should().Be("Engineering");
            department.Description.Should().Be("CS Dept");
            department.IsActive.Should().BeTrue();
        }
    }
}
