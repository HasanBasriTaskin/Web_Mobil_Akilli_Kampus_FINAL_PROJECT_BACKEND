using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Cafeteria;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Meal
{
    public class CafeteriaCreateValidator : AbstractValidator<CafeteriaCreateDto>
    {
        public CafeteriaCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Yemekhane adı zorunludur")
                .MaximumLength(100)
                .WithMessage("Yemekhane adı en fazla 100 karakter olabilir");

            RuleFor(x => x.Location)
                .NotEmpty()
                .WithMessage("Konum zorunludur")
                .MaximumLength(200)
                .WithMessage("Konum en fazla 200 karakter olabilir");

            RuleFor(x => x.Capacity)
                .GreaterThan(0)
                .WithMessage("Kapasite 0'dan büyük olmalıdır")
                .LessThanOrEqualTo(5000)
                .WithMessage("Kapasite en fazla 5000 olabilir");
        }
    }

    public class CafeteriaUpdateValidator : AbstractValidator<CafeteriaUpdateDto>
    {
        public CafeteriaUpdateValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage("Yemekhane adı en fazla 100 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Location)
                .MaximumLength(200)
                .WithMessage("Konum en fazla 200 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.Location));

            RuleFor(x => x.Capacity)
                .GreaterThan(0)
                .WithMessage("Kapasite 0'dan büyük olmalıdır")
                .When(x => x.Capacity.HasValue);
        }
    }
}
