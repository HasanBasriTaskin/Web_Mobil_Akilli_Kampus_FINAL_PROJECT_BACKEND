using FluentAssertions;
using SMARTCAMPUS.BusinessLayer.ValidationRules.Auth;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;
using Xunit;

namespace SMARTCAMPUS.Tests.ValidationRules.Auth
{
    public class ResetPasswordValidatorTests
    {
        private readonly ResetPasswordValidator _validator;

        public ResetPasswordValidatorTests()
        {
            _validator = new ResetPasswordValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new ResetPasswordDto
            {
                Email = "test@example.com",
                Token = "valid-token",
                NewPassword = "Password123!",
                ConfirmPassword = "Password123!"
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldFail_WhenPasswordsDoNotMatch()
        {
            var dto = new ResetPasswordDto
            {
                Email = "test@example.com",
                Token = "valid-token",
                NewPassword = "Password123!",
                ConfirmPassword = "DifferentPassword"
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
             var dto = new ResetPasswordDto
             {
                 Email = email,
                 Token = "valid-token",
                 NewPassword = "Password123!",
                 ConfirmPassword = "Password123!"
             };
             var result = _validator.Validate(dto);
             result.IsValid.Should().BeFalse();
         }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_ShouldFail_WhenTokenIsMissing(string token)
        {
             var dto = new ResetPasswordDto
             {
                 Email = "test@example.com",
                 Token = token,
                 NewPassword = "Password123!",
                 ConfirmPassword = "Password123!"
             };
             var result = _validator.Validate(dto);
             result.IsValid.Should().BeFalse();
         }
    }
}
