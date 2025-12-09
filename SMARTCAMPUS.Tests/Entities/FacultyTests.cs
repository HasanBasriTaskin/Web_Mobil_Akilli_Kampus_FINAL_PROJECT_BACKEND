using FluentAssertions;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Entities
{
    public class FacultyTests
    {
        [Fact]
        public void Faculty_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var faculty = new Faculty
            {
                Id = 1,
                EmployeeNumber = "EMP001",
                Title = "Prof.",
                OfficeLocation = "A-101",
                UserId = "1",
                User = new User(),
                DepartmentId = 1,
                Department = new Department()
            };

            // Assert
            faculty.Id.Should().Be(1);
            faculty.EmployeeNumber.Should().Be("EMP001");
            faculty.Title.Should().Be("Prof.");
            faculty.OfficeLocation.Should().Be("A-101");
            faculty.UserId.Should().Be("1");
            faculty.DepartmentId.Should().Be(1);
        }
    }
}
