using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Event;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Event
{
    public class EventCategoryCreateValidator : AbstractValidator<EventCategoryCreateDto>
    {
        public EventCategoryCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Kategori adı zorunludur")
                .MaximumLength(100)
                .WithMessage("Kategori adı en fazla 100 karakter olabilir");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Açıklama en fazla 500 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.IconName)
                .MaximumLength(50)
                .WithMessage("İkon adı en fazla 50 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.IconName));
        }
    }

    public class EventCategoryUpdateValidator : AbstractValidator<EventCategoryUpdateDto>
    {
        public EventCategoryUpdateValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage("Kategori adı en fazla 100 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Açıklama en fazla 500 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}
