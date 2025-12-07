using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Auth
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MinimumLength(2).WithMessage("Full Name is required.");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid email is required.");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters.");
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match.");
        }
    }
}
