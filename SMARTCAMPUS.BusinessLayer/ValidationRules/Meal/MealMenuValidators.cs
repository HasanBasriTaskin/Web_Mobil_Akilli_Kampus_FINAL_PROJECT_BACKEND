using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Menu;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Meal
{
    public class MealMenuCreateValidator : AbstractValidator<MealMenuCreateDto>
    {
        public MealMenuCreateValidator()
        {
            RuleFor(x => x.CafeteriaId)
                .GreaterThan(0)
                .WithMessage("Yemekhane seçimi zorunludur");

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage("Tarih zorunludur")
                .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("Geçmiş tarihte menü oluşturulamaz");

            RuleFor(x => x.MealType)
                .IsInEnum()
                .WithMessage("Geçersiz öğün tipi");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Fiyat 0 veya daha büyük olmalıdır");

            RuleFor(x => x.FoodItemIds)
                .NotEmpty()
                .WithMessage("En az bir yemek seçilmelidir")
                .Must(x => x != null && x.Count > 0)
                .WithMessage("En az bir yemek seçilmelidir");
        }
    }
}
