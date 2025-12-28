using FluentAssertions;
using FluentValidation.TestHelper;
using SMARTCAMPUS.BusinessLayer.ValidationRules.Scheduling;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;
using Xunit;

namespace SMARTCAMPUS.Tests.ValidationRules.Scheduling
{
    public class ClassroomReservationCreateValidatorTests
    {
        private readonly ClassroomReservationCreateValidator _validator;

        public ClassroomReservationCreateValidatorTests()
        {
            _validator = new ClassroomReservationCreateValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            // Get next weekday (not weekend)
            var nextWeekday = DateTime.Today.AddDays(1);
            while (nextWeekday.DayOfWeek == DayOfWeek.Saturday || nextWeekday.DayOfWeek == DayOfWeek.Sunday)
            {
                nextWeekday = nextWeekday.AddDays(1);
            }

            var dto = new ClassroomReservationCreateDto
            {
                ClassroomId = 1,
                Purpose = "Toplantı",
                ReservationDate = nextWeekday,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11)
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldFail_WhenClassroomIdIsInvalid()
        {
            var dto = new ClassroomReservationCreateDto { ClassroomId = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.ClassroomId);
        }

        [Fact]
        public void Validate_ShouldFail_WhenReservationDateIsWeekend()
        {
            var saturday = DateTime.Today.AddDays(((int)DayOfWeek.Saturday - (int)DateTime.Today.DayOfWeek + 7) % 7);
            var dto = new ClassroomReservationCreateDto { ReservationDate = saturday };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.ReservationDate);
        }

        [Fact]
        public void Validate_ShouldFail_WhenStartTimeIsOutOfRange()
        {
            var dto = new ClassroomReservationCreateDto { StartTime = TimeSpan.FromHours(7) };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.StartTime);
        }

        [Fact]
        public void Validate_ShouldFail_WhenEndTimeIsBeforeStartTime()
        {
            var dto = new ClassroomReservationCreateDto
            {
                StartTime = TimeSpan.FromHours(11),
                EndTime = TimeSpan.FromHours(10)
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.EndTime);
        }

        [Fact]
        public void Validate_ShouldFail_WhenDurationIsLessThan30Minutes()
        {
            var dto = new ClassroomReservationCreateDto
            {
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromMinutes(10).Add(TimeSpan.FromMinutes(20))
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.EndTime);
        }
    }
}

