using FluentAssertions;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Entities
{
    public class StudentTests
    {
        [Fact]
        public void Student_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var student = new Student
            {
                Id = 1,
                UserId = "100",
                User = new User { UserName = "student1" },
                DepartmentId = 5,
                Department = new Department { Name = "CS" },
                StudentNumber = "2023001",
                GPA = 3.5,
                CGPA = 3.6
            };

            // Assert
            student.Id.Should().Be(1);
            student.UserId.Should().Be("100");
            student.User.Should().NotBeNull();
            student.DepartmentId.Should().Be(5);
            student.Department.Should().NotBeNull();
            student.StudentNumber.Should().Be("2023001");
            student.GPA.Should().Be(3.5);
            student.CGPA.Should().Be(3.6);
        }
    }
}
