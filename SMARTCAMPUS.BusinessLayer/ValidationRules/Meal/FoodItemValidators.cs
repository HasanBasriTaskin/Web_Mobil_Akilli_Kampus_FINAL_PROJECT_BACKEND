using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.FoodItem;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Meal
{
    public class FoodItemCreateValidator : AbstractValidator<FoodItemCreateDto>
    {
        public FoodItemCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Yemek adı zorunludur")
                .MaximumLength(100)
                .WithMessage("Yemek adı en fazla 100 karakter olabilir");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Açıklama en fazla 500 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Category)
                .IsInEnum()
                .WithMessage("Geçersiz kategori");

            RuleFor(x => x.Calories)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Kalori 0 veya daha büyük olmalıdır")
                .LessThanOrEqualTo(5000)
                .WithMessage("Kalori en fazla 5000 olabilir")
                .When(x => x.Calories.HasValue);
        }
    }

    public class FoodItemUpdateValidator : AbstractValidator<FoodItemUpdateDto>
    {
        public FoodItemUpdateValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage("Yemek adı en fazla 100 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Açıklama en fazla 500 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Category)
                .IsInEnum()
                .WithMessage("Geçersiz kategori")
                .When(x => x.Category.HasValue);

            RuleFor(x => x.Calories)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Kalori 0 veya daha büyük olmalıdır")
                .When(x => x.Calories.HasValue);
        }
    }
}
