using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Academic
{
    public class GradeUpdateValidator : AbstractValidator<GradeUpdateDto>
    {
        public GradeUpdateValidator()
        {
            RuleFor(x => x.EnrollmentId)
                .GreaterThan(0)
                .WithMessage("Enrollment ID must be greater than 0");

            RuleFor(x => x.MidtermGrade)
                .InclusiveBetween(0, 100)
                .WithMessage("Midterm grade must be between 0 and 100")
                .When(x => x.MidtermGrade.HasValue);

            RuleFor(x => x.FinalGrade)
                .InclusiveBetween(0, 100)
                .WithMessage("Final grade must be between 0 and 100")
                .When(x => x.FinalGrade.HasValue);

            RuleFor(x => x)
                .Must(x => x.MidtermGrade.HasValue || x.FinalGrade.HasValue)
                .WithMessage("At least one grade (Midterm or Final) must be provided");
        }
    }

    public class GradeBulkUpdateValidator : AbstractValidator<GradeBulkUpdateDto>
    {
        public GradeBulkUpdateValidator()
        {
            RuleFor(x => x.Grades)
                .NotEmpty()
                .WithMessage("Grades list cannot be empty");

            RuleForEach(x => x.Grades)
                .SetValidator(new GradeUpdateValidator());
        }
    }
}

