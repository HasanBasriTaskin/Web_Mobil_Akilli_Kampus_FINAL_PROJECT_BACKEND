using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Auth
{
    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Şifre zorunludur.");
        }
    }
}
