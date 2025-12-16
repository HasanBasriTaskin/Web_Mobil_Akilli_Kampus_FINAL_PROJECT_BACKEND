using FluentAssertions;
using SMARTCAMPUS.BusinessLayer.ValidationRules.User;
using SMARTCAMPUS.EntityLayer.DTOs.User;
using Xunit;

namespace SMARTCAMPUS.Tests.ValidationRules.User
{
    public class UserUpdateValidatorTests
    {
        private readonly UserUpdateValidator _validator;

        public UserUpdateValidatorTests()
        {
            _validator = new UserUpdateValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new UserUpdateDto
            {
                FullName = "John Doe",
                Email = "john@example.com"
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_ShouldFail_WhenFullNameIsMissing(string fullName)
        {
            var dto = new UserUpdateDto
            {
                FullName = fullName,
                Email = "john@example.com"
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("invalid-email")]
        public void Validate_ShouldFail_WhenEmailIsInvalid(string email)
        {
            var dto = new UserUpdateDto
            {
                FullName = "John Doe",
                Email = email
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
        }
    }
}
