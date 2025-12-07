using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.User;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.User
{
    public class UserUpdateValidator : AbstractValidator<UserUpdateDto>
    {
        public UserUpdateValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().WithMessage("Ad Soyad zorunludur.").MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
            RuleFor(x => x.PhoneNumber).Matches(@"^\+?\d{10,15}$").WithMessage("Geçersiz telefon numarası.").When(x => !string.IsNullOrEmpty(x.PhoneNumber));
        }
    }
}
