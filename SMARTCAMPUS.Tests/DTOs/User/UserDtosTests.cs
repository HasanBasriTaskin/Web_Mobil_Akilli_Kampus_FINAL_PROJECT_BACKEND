using FluentAssertions;
using SMARTCAMPUS.EntityLayer.DTOs.User;
using Xunit;

namespace SMARTCAMPUS.Tests.DTOs.User
{
    public class UserListDtoTests
    {
        [Fact]
        public void UserListDto_ShouldInitializePropertiesCorrectly()
        {
            var dto = new UserListDto
            {
                Id = "1",
                FullName = "John Doe",
                Email = "john@example.com",
                PhoneNumber = "123456",
                IsActive = true,
                Roles = new List<string> { "Admin" }
            };

            dto.Id.Should().Be("1");
            dto.FullName.Should().Be("John Doe");
            dto.Email.Should().Be("john@example.com");
            dto.PhoneNumber.Should().Be("123456");
            dto.IsActive.Should().BeTrue();
            dto.Roles.Should().Contain("Admin");
        }
    }

    public class UserProfileDtoTests
    {
        [Fact]
        public void UserProfileDto_ShouldInitializePropertiesCorrectly()
        {
            var dto = new UserProfileDto
            {
                IdString = "1",
                FullName = "John Doe",
                Email = "john@example.com",
                ProfilePictureUrl = "url",
                Roles = new List<string> { "Student" }
            };

            dto.IdString.Should().Be("1");
            dto.FullName.Should().Be("John Doe");
            dto.Email.Should().Be("john@example.com");
            dto.ProfilePictureUrl.Should().Be("url");
            dto.Roles.Should().Contain("Student");
        }
    }

    public class UserQueryParametersTests
    {
        [Fact]
        public void UserQueryParameters_ShouldInitializePropertiesCorrectly()
        {
            var parameters = new UserQueryParameters
            {
                Page = 2,
                Limit = 20,
                Search = "test",
                Role = "Admin",
                DepartmentId = 5
            };

            parameters.Page.Should().Be(2);
            parameters.Limit.Should().Be(20);
            parameters.Search.Should().Be("test");
            parameters.Role.Should().Be("Admin");
            parameters.DepartmentId.Should().Be(5);
        }

        [Fact]
        public void UserQueryParameters_ShouldCapLimit()
        {
            var parameters = new UserQueryParameters
            {
                Limit = 100 // Max is 50
            };
            parameters.Limit.Should().Be(50);
        }
    }

    public class UserUpdateDtoTests
    {
        [Fact]
        public void UserUpdateDto_ShouldInitializePropertiesCorrectly()
        {
            var dto = new UserUpdateDto
            {
                FullName = "New Name",
                Email = "new@example.com",
                PhoneNumber = "999"
            };

            dto.FullName.Should().Be("New Name");
            dto.Email.Should().Be("new@example.com");
            dto.PhoneNumber.Should().Be("999");
        }
    }
}
