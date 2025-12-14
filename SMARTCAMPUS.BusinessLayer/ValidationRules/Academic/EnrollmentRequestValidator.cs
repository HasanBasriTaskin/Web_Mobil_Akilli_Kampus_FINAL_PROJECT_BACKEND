using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Academic
{
    public class EnrollmentRequestValidator : AbstractValidator<EnrollmentRequestDto>
    {
        public EnrollmentRequestValidator()
        {
            RuleFor(x => x.SectionId)
                .GreaterThan(0)
                .WithMessage("Section ID must be greater than 0");
        }
    }
}

