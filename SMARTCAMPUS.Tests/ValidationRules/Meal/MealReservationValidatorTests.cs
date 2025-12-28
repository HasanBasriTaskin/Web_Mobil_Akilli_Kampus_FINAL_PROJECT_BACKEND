using FluentAssertions;
using FluentValidation.TestHelper;
using SMARTCAMPUS.BusinessLayer.ValidationRules.Meal;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Reservation;
using SMARTCAMPUS.EntityLayer.Enums;
using Xunit;

namespace SMARTCAMPUS.Tests.ValidationRules.Meal
{
    public class MealReservationCreateValidatorTests
    {
        private readonly MealReservationCreateValidator _validator;

        public MealReservationCreateValidatorTests()
        {
            _validator = new MealReservationCreateValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new MealReservationCreateDto
            {
                MenuId = 1,
                CafeteriaId = 1,
                MealType = MealType.Lunch,
                Date = DateTime.Today
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldFail_WhenMenuIdIsInvalid()
        {
            var dto = new MealReservationCreateDto { MenuId = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.MenuId);
        }

        [Fact]
        public void Validate_ShouldFail_WhenDateIsInPast()
        {
            var dto = new MealReservationCreateDto { Date = DateTime.Today.AddDays(-1) };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Date);
        }
    }
}

