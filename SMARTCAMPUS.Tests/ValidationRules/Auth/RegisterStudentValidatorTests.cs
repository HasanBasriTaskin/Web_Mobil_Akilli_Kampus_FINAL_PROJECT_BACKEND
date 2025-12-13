using FluentAssertions;
using SMARTCAMPUS.BusinessLayer.ValidationRules.Auth;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;
using Xunit;

namespace SMARTCAMPUS.Tests.ValidationRules.Auth
{
    public class RegisterStudentValidatorTests
    {
        private readonly RegisterStudentValidator _validator;

        public RegisterStudentValidatorTests()
        {
            _validator = new RegisterStudentValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new RegisterStudentDto
            {
                FullName = "John Doe",
                Email = "john.doe@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                DepartmentId = 1,
                StudentNumber = "12345"
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_ShouldFail_WhenFullNameIsMissing(string fullName)
        {
            var dto = new RegisterStudentDto { FullName = fullName };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("invalid-email")]
        public void Validate_ShouldFail_WhenEmailIsInvalid(string email)
        {
            var dto = new RegisterStudentDto { FullName = "John Doe", Email = email };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("short")] // Assuming min length
        public void Validate_ShouldFail_WhenPasswordIsInvalid(string password)
        {
             var dto = new RegisterStudentDto
             {
                 FullName = "John Doe",
                 Email = "john.doe@example.com",
                 Password = password
             };
             var result = _validator.Validate(dto);
             result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_ShouldFail_WhenDepartmentIdIsInvalid()
        {
            var dto = new RegisterStudentDto
            {
                 FullName = "John Doe",
                 Email = "john.doe@example.com",
                 Password = "Password123!",
                 ConfirmPassword = "Password123!",
                 DepartmentId = 0
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_ShouldFail_WhenStudentNumberIsMissing(string studentNumber)
        {
            var dto = new RegisterStudentDto
            {
                 FullName = "John Doe",
                 Email = "john.doe@example.com",
                 Password = "Password123!",
                 ConfirmPassword = "Password123!",
                 DepartmentId = 1,
                 StudentNumber = studentNumber
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
        }
    }
}
