using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Auth
{
    public class RegisterStudentValidator : AbstractValidator<RegisterStudentDto>
    {
        public RegisterStudentValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MinimumLength(2).WithMessage("Ad Soyad zorunludur ve en az 2 karakter olmalıdır.");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Şifreler uyuşmuyor.");
            RuleFor(x => x.StudentNumber).NotEmpty().WithMessage("Öğrenci Numarası zorunludur.");
            RuleFor(x => x.DepartmentId).GreaterThan(0).WithMessage("Bölüm seçimi zorunludur.");
        }
    }
}
