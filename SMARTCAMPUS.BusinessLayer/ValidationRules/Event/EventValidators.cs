using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Event;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Event
{
    public class EventCreateValidator : AbstractValidator<EventCreateDto>
    {
        public EventCreateValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Etkinlik başlığı zorunludur")
                .MaximumLength(200)
                .WithMessage("Başlık en fazla 200 karakter olabilir");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Açıklama zorunludur")
                .MaximumLength(2000)
                .WithMessage("Açıklama en fazla 2000 karakter olabilir");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                .WithMessage("Kategori seçimi zorunludur");

            RuleFor(x => x.StartDate)
                .NotEmpty()
                .WithMessage("Başlangıç tarihi zorunludur")
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Başlangıç tarihi gelecekte olmalıdır");

            RuleFor(x => x.EndDate)
                .NotEmpty()
                .WithMessage("Bitiş tarihi zorunludur")
                .GreaterThan(x => x.StartDate)
                .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır");

            RuleFor(x => x.Location)
                .NotEmpty()
                .WithMessage("Konum zorunludur")
                .MaximumLength(300)
                .WithMessage("Konum en fazla 300 karakter olabilir");

            RuleFor(x => x.Capacity)
                .GreaterThan(0)
                .WithMessage("Kapasite 0'dan büyük olmalıdır")
                .LessThanOrEqualTo(10000)
                .WithMessage("Kapasite en fazla 10.000 olabilir");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Fiyat 0 veya daha büyük olmalıdır");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500)
                .WithMessage("Resim URL'i en fazla 500 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));
        }
    }

    public class EventUpdateValidator : AbstractValidator<EventUpdateDto>
    {
        public EventUpdateValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(200)
                .WithMessage("Başlık en fazla 200 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.Title));

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Açıklama en fazla 2000 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate.GetValueOrDefault())
                .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

            RuleFor(x => x.Capacity)
                .GreaterThan(0)
                .WithMessage("Kapasite 0'dan büyük olmalıdır")
                .When(x => x.Capacity.HasValue);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Fiyat 0 veya daha büyük olmalıdır")
                .When(x => x.Price.HasValue);
        }
    }
}
