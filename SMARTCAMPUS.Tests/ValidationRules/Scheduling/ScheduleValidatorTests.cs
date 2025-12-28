using FluentAssertions;
using FluentValidation.TestHelper;
using SMARTCAMPUS.BusinessLayer.ValidationRules.Scheduling;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;
using Xunit;

namespace SMARTCAMPUS.Tests.ValidationRules.Scheduling
{
    public class ScheduleCreateValidatorTests
    {
        private readonly ScheduleCreateValidator _validator;

        public ScheduleCreateValidatorTests()
        {
            _validator = new ScheduleCreateValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new ScheduleCreateDto
            {
                SectionId = 1,
                ClassroomId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(10)
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldFail_WhenSectionIdIsInvalid()
        {
            var dto = new ScheduleCreateDto { SectionId = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.SectionId);
        }

        [Fact]
        public void Validate_ShouldFail_WhenStartTimeIsOutOfRange()
        {
            var dto = new ScheduleCreateDto { StartTime = TimeSpan.FromHours(7) };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.StartTime);
        }

        [Fact]
        public void Validate_ShouldFail_WhenDurationIsLessThan40Minutes()
        {
            var dto = new ScheduleCreateDto
            {
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromMinutes(9).Add(TimeSpan.FromMinutes(30))
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.EndTime);
        }
    }

    public class ScheduleUpdateValidatorTests
    {
        private readonly ScheduleUpdateValidator _validator;

        public ScheduleUpdateValidatorTests()
        {
            _validator = new ScheduleUpdateValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new ScheduleUpdateDto
            {
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(10)
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldFail_WhenClassroomIdIsInvalid()
        {
            var dto = new ScheduleUpdateDto { ClassroomId = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.ClassroomId);
        }

        [Fact]
        public void Validate_ShouldFail_WhenEndTimeIsBeforeStartTime()
        {
            var dto = new ScheduleUpdateDto
            {
                StartTime = TimeSpan.FromHours(11),
                EndTime = TimeSpan.FromHours(10)
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.EndTime);
        }
    }
}

