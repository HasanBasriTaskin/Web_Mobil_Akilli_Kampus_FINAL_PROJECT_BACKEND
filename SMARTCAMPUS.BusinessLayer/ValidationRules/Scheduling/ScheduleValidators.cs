using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Scheduling
{
    public class ScheduleCreateValidator : AbstractValidator<ScheduleCreateDto>
    {
        public ScheduleCreateValidator()
        {
            RuleFor(x => x.SectionId)
                .GreaterThan(0)
                .WithMessage("Ders bölümü seçimi zorunludur");

            RuleFor(x => x.ClassroomId)
                .GreaterThan(0)
                .WithMessage("Sınıf seçimi zorunludur");

            RuleFor(x => x.DayOfWeek)
                .IsInEnum()
                .WithMessage("Geçersiz gün seçimi");

            RuleFor(x => x.StartTime)
                .NotEmpty()
                .WithMessage("Başlangıç saati zorunludur")
                .Must(t => t >= TimeSpan.FromHours(8) && t <= TimeSpan.FromHours(22))
                .WithMessage("Ders saati 08:00 - 22:00 arasında olmalıdır");

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .WithMessage("Bitiş saati zorunludur")
                .GreaterThan(x => x.StartTime)
                .WithMessage("Bitiş saati başlangıç saatinden sonra olmalıdır")
                .Must((dto, endTime) => (endTime - dto.StartTime).TotalMinutes >= 40)
                .WithMessage("Ders süresi en az 40 dakika olmalıdır")
                .Must((dto, endTime) => (endTime - dto.StartTime).TotalHours <= 4)
                .WithMessage("Ders süresi en fazla 4 saat olabilir");
        }
    }

    public class ScheduleUpdateValidator : AbstractValidator<ScheduleUpdateDto>
    {
        public ScheduleUpdateValidator()
        {
            RuleFor(x => x.ClassroomId)
                .GreaterThan(0)
                .WithMessage("Geçersiz sınıf seçimi")
                .When(x => x.ClassroomId.HasValue);

            RuleFor(x => x.DayOfWeek)
                .IsInEnum()
                .WithMessage("Geçersiz gün seçimi")
                .When(x => x.DayOfWeek.HasValue);

            RuleFor(x => x.StartTime)
                .Must(t => !t.HasValue || (t.Value >= TimeSpan.FromHours(8) && t.Value <= TimeSpan.FromHours(22)))
                .WithMessage("Ders saati 08:00 - 22:00 arasında olmalıdır")
                .When(x => x.StartTime.HasValue);

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime.GetValueOrDefault())
                .WithMessage("Bitiş saati başlangıç saatinden sonra olmalıdır")
                .When(x => x.StartTime.HasValue && x.EndTime.HasValue);
        }
    }
}
