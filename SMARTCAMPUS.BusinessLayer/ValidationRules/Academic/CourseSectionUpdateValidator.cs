using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Academic
{
    public class CourseSectionUpdateValidator : AbstractValidator<CourseSectionUpdateDto>
    {
        public CourseSectionUpdateValidator()
        {
            RuleFor(x => x.SectionNumber)
                .MaximumLength(10)
                .WithMessage("Section number must not exceed 10 characters")
                .When(x => !string.IsNullOrEmpty(x.SectionNumber));

            RuleFor(x => x.Semester)
                .MaximumLength(20)
                .WithMessage("Semester must not exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.Semester));

            RuleFor(x => x.Year)
                .GreaterThan(2000)
                .WithMessage("Year must be greater than 2000")
                .LessThanOrEqualTo(2100)
                .WithMessage("Year must not exceed 2100")
                .When(x => x.Year.HasValue);

            RuleFor(x => x.Capacity)
                .GreaterThan(0)
                .WithMessage("Capacity must be greater than 0")
                .LessThanOrEqualTo(500)
                .WithMessage("Capacity must not exceed 500")
                .When(x => x.Capacity.HasValue);

            RuleFor(x => x.ScheduleJson)
                .MaximumLength(2000)
                .WithMessage("Schedule JSON must not exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.ScheduleJson));
        }
    }
}

