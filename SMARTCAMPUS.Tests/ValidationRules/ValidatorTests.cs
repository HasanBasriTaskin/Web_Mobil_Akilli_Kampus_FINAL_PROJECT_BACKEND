using FluentValidation.TestHelper;
using SMARTCAMPUS.BusinessLayer.ValidationRules.Academic;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using Xunit;

namespace SMARTCAMPUS.Tests.ValidationRules
{
    public class ValidatorTests
    {
        [Fact]
        public void AttendanceCheckInValidator_ShouldHaveError_WhenCoordinatesInvalid()
        {
            var validator = new AttendanceCheckInValidator();
            var dto = new AttendanceCheckInDto { Latitude = 200, Longitude = 200 }; // Invalid

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Latitude);
            result.ShouldHaveValidationErrorFor(x => x.Longitude);
        }

        [Fact]
        public void CourseCreateValidator_ShouldHaveError_WhenCodeEmpty()
        {
            var validator = new CourseCreateValidator();
            var dto = new CourseCreateDto { Code = "", Name = "Name", DepartmentId = 1 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public void CourseSectionCreateValidator_ShouldHaveError_WhenSemesterEmpty()
        {
            var validator = new CourseSectionCreateValidator();
            var dto = new CourseSectionCreateDto { CourseId = 1, InstructorId = "1", Semester = "", Year = 2024 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Semester);
        }

        [Fact]
        public void EnrollmentRequestValidator_ShouldHaveError_WhenSectionIdZero()
        {
            var validator = new EnrollmentRequestValidator();
            var dto = new EnrollmentRequestDto { SectionId = 0 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SectionId);
        }
    }
}
