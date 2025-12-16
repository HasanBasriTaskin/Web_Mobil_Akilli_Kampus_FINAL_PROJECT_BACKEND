using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Academic
{
    public class CourseSectionCreateValidator : AbstractValidator<CourseSectionCreateDto>
    {
        public CourseSectionCreateValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0)
                .WithMessage("Course ID must be greater than 0");

            RuleFor(x => x.SectionNumber)
                .NotEmpty()
                .WithMessage("Section number is required")
                .MaximumLength(10)
                .WithMessage("Section number must not exceed 10 characters");

            RuleFor(x => x.Semester)
                .NotEmpty()
                .WithMessage("Semester is required")
                .MaximumLength(20)
                .WithMessage("Semester must not exceed 20 characters");

            RuleFor(x => x.Year)
                .GreaterThan(2000)
                .WithMessage("Year must be greater than 2000")
                .LessThanOrEqualTo(2100)
                .WithMessage("Year must not exceed 2100");

            RuleFor(x => x.Capacity)
                .GreaterThan(0)
                .WithMessage("Capacity must be greater than 0")
                .LessThanOrEqualTo(500)
                .WithMessage("Capacity must not exceed 500");

            RuleFor(x => x.ScheduleJson)
                .MaximumLength(2000)
                .WithMessage("Schedule JSON must not exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.ScheduleJson));
        }
    }
}

