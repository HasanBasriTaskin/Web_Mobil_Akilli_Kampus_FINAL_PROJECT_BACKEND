using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Academic
{
    public class CourseUpdateValidator : AbstractValidator<CourseUpdateDto>
    {
        public CourseUpdateValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(200)
                .WithMessage("Course name must not exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Description must not exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Credits)
                .GreaterThan(0)
                .WithMessage("Credits must be greater than 0")
                .LessThanOrEqualTo(10)
                .WithMessage("Credits must not exceed 10")
                .When(x => x.Credits.HasValue);

            RuleFor(x => x.ECTS)
                .GreaterThan(0)
                .WithMessage("ECTS must be greater than 0")
                .LessThanOrEqualTo(30)
                .WithMessage("ECTS must not exceed 30")
                .When(x => x.ECTS.HasValue);

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .WithMessage("Department ID must be greater than 0")
                .When(x => x.DepartmentId.HasValue);

            RuleFor(x => x.SyllabusUrl)
                .MaximumLength(500)
                .WithMessage("Syllabus URL must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.SyllabusUrl));
        }
    }
}

