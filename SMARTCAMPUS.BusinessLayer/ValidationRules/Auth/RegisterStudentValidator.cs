using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Auth
{
    public class RegisterStudentValidator : AbstractValidator<RegisterStudentDto>
    {
        public RegisterStudentValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MinimumLength(2).WithMessage("Full Name is required.");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid email is required.");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters.");
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match.");
            RuleFor(x => x.StudentNumber).NotEmpty().WithMessage("Student Number is required.");
            RuleFor(x => x.DepartmentId).GreaterThan(0).WithMessage("Department is required.");
        }
    }
}
