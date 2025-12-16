using FluentAssertions;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;
using Xunit;

namespace SMARTCAMPUS.Tests.DTOs.Auth
{
    public class ChangePasswordDtoTests
    {
        [Fact]
        public void ChangePasswordDto_ShouldInitializePropertiesCorrectly()
        {
            var dto = new ChangePasswordDto
            {
                OldPassword = "old",
                NewPassword = "new",
                ConfirmNewPassword = "new"
            };

            dto.OldPassword.Should().Be("old");
            dto.NewPassword.Should().Be("new");
            dto.ConfirmNewPassword.Should().Be("new");
        }
    }

    public class ForgotPasswordDtoTests
    {
        [Fact]
        public void ForgotPasswordDto_ShouldInitializePropertiesCorrectly()
        {
            var dto = new ForgotPasswordDto { Email = "test@test.com" };
            dto.Email.Should().Be("test@test.com");
        }
    }

    public class LoginDtoTests
    {
        [Fact]
        public void LoginDto_ShouldInitializePropertiesCorrectly()
        {
            var dto = new LoginDto { Email = "test@test.com", Password = "pass" };
            dto.Email.Should().Be("test@test.com");
            dto.Password.Should().Be("pass");
        }
    }

    public class RefreshTokenDtoTests
    {
        [Fact]
        public void RefreshTokenDto_ShouldInitializePropertiesCorrectly()
        {
            var dto = new RefreshTokenDto { Token = "token123" };
            dto.Token.Should().Be("token123");
        }
    }

    public class RegisterStudentDtoTests
    {
        [Fact]
        public void RegisterStudentDto_ShouldInitializePropertiesCorrectly()
        {
            var dto = new RegisterStudentDto
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "pass",
                ConfirmPassword = "pass",
                DepartmentId = 1,
                StudentNumber = "123"
            };

            dto.FullName.Should().Be("John Doe");
            dto.Email.Should().Be("john@example.com");
            dto.Password.Should().Be("pass");
            dto.ConfirmPassword.Should().Be("pass");
            dto.DepartmentId.Should().Be(1);
            dto.StudentNumber.Should().Be("123");
        }
    }

    public class RegisterUserDtoTests
    {
        [Fact]
        public void RegisterUserDto_ShouldInitializePropertiesCorrectly()
        {
            var dto = new RegisterUserDto
            {
                FullName = "Jane Doe",
                Email = "jane@example.com",
                Password = "pass",
                ConfirmPassword = "pass",
                UserType = "Faculty",
                DepartmentId = 1,
                EmployeeNumber = "E001",
                Title = "Prof",
                OfficeLocation = "B101"
            };

            dto.FullName.Should().Be("Jane Doe");
            dto.Email.Should().Be("jane@example.com");
            dto.Password.Should().Be("pass");
            dto.ConfirmPassword.Should().Be("pass");
            dto.UserType.Should().Be("Faculty");
            dto.DepartmentId.Should().Be(1);
            dto.EmployeeNumber.Should().Be("E001");
            dto.Title.Should().Be("Prof");
            dto.OfficeLocation.Should().Be("B101");
        }
    }

    public class ResetPasswordDtoTests
    {
        [Fact]
        public void ResetPasswordDto_ShouldInitializePropertiesCorrectly()
        {
            var dto = new ResetPasswordDto
            {
                Email = "email",
                Token = "token",
                NewPassword = "new",
                ConfirmPassword = "new"
            };

            dto.Email.Should().Be("email");
            dto.Token.Should().Be("token");
            dto.NewPassword.Should().Be("new");
            dto.ConfirmPassword.Should().Be("new");
        }
    }

    public class TokenDtoTests
    {
        [Fact]
        public void TokenDto_ShouldInitializePropertiesCorrectly()
        {
            var dto = new TokenDto
            {
                AccessToken = "access",
                RefreshToken = "refresh",
                AccessTokenExpiration = DateTime.MaxValue,
                RefreshTokenExpiration = DateTime.MaxValue
            };

            dto.AccessToken.Should().Be("access");
            dto.RefreshToken.Should().Be("refresh");
        }
    }
}
