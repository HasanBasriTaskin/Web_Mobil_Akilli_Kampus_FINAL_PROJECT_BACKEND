using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Scheduling
{
    public class ClassroomReservationCreateValidator : AbstractValidator<ClassroomReservationCreateDto>
    {
        public ClassroomReservationCreateValidator()
        {
            RuleFor(x => x.ClassroomId)
                .GreaterThan(0)
                .WithMessage("Sınıf seçimi zorunludur");

            RuleFor(x => x.ReservationDate)
                .NotEmpty()
                .WithMessage("Tarih zorunludur")
                .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("Geçmiş tarihte rezervasyon yapılamaz")
                .Must(date => date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                .WithMessage("Hafta sonu rezervasyon yapılamaz");

            RuleFor(x => x.StartTime)
                .NotEmpty()
                .WithMessage("Başlangıç saati zorunludur")
                .Must(t => t >= TimeSpan.FromHours(8) && t <= TimeSpan.FromHours(22))
                .WithMessage("Rezervasyon saati 08:00 - 22:00 arasında olmalıdır");

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .WithMessage("Bitiş saati zorunludur")
                .GreaterThan(x => x.StartTime)
                .WithMessage("Bitiş saati başlangıç saatinden sonra olmalıdır")
                .Must((dto, endTime) => (endTime - dto.StartTime).TotalMinutes >= 30)
                .WithMessage("Rezervasyon süresi en az 30 dakika olmalıdır")
                .Must((dto, endTime) => (endTime - dto.StartTime).TotalHours <= 4)
                .WithMessage("Rezervasyon süresi en fazla 4 saat olabilir");

            RuleFor(x => x.Purpose)
                .NotEmpty()
                .WithMessage("Amaç zorunludur")
                .MaximumLength(500)
                .WithMessage("Amaç en fazla 500 karakter olabilir");

            RuleFor(x => x.StudentLeaderName)
                .MaximumLength(100)
                .WithMessage("Öğrenci temsilcisi adı en fazla 100 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.StudentLeaderName));
        }
    }
}
