using FluentAssertions;
using SMARTCAMPUS.BusinessLayer.ValidationRules.Auth;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;
using Xunit;

namespace SMARTCAMPUS.Tests.ValidationRules.Auth
{
    public class LoginValidatorTests
    {
        private readonly LoginValidator _validator;

        public LoginValidatorTests()
        {
            _validator = new LoginValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenLoginDtoIsValid()
        {
            var dto = new LoginDto { Email = "test@example.com", Password = "password123" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldReturnFalse_WhenEmailIsEmpty()
        {
            var dto = new LoginDto { Email = "", Password = "password123" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_ShouldReturnFalse_WhenEmailIsInvalid()
        {
             var dto = new LoginDto { Email = "invalid-email", Password = "password123" };
             var result = _validator.Validate(dto);
             result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_ShouldReturnFalse_WhenPasswordIsEmpty()
        {
            var dto = new LoginDto { Email = "test@example.com", Password = "" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
        }
    }
}
