using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.User;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.User
{
    public class UserUpdateValidator : AbstractValidator<UserUpdateDto>
    {
        public UserUpdateValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().WithMessage("Full Name is required").MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid Email is required");
            RuleFor(x => x.PhoneNumber).Matches(@"^\+?\d{10,15}$").WithMessage("Invalid Phone Number").When(x => !string.IsNullOrEmpty(x.PhoneNumber));
        }
    }
}
