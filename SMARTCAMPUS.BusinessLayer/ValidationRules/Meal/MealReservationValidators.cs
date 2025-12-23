using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Reservation;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Meal
{
    public class MealReservationCreateValidator : AbstractValidator<MealReservationCreateDto>
    {
        public MealReservationCreateValidator()
        {
            RuleFor(x => x.MenuId)
                .GreaterThan(0)
                .WithMessage("Menü seçimi zorunludur");

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage("Tarih zorunludur")
                .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("Geçmiş tarihte rezervasyon yapılamaz");

            RuleFor(x => x.MealType)
                .IsInEnum()
                .WithMessage("Geçersiz öğün tipi");
        }
    }
}
