using FluentAssertions;
using FluentValidation.TestHelper;
using SMARTCAMPUS.BusinessLayer.ValidationRules.Meal;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Menu;
using SMARTCAMPUS.EntityLayer.Enums;
using Xunit;

namespace SMARTCAMPUS.Tests.ValidationRules.Meal
{
    public class MealMenuCreateValidatorTests
    {
        private readonly MealMenuCreateValidator _validator;

        public MealMenuCreateValidatorTests()
        {
            _validator = new MealMenuCreateValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new MealMenuCreateDto
            {
                CafeteriaId = 1,
                Date = DateTime.Today,
                MealType = MealType.Lunch,
                Price = 50,
                FoodItemIds = new List<int> { 1, 2 }
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldFail_WhenCafeteriaIdIsInvalid()
        {
            var dto = new MealMenuCreateDto { CafeteriaId = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.CafeteriaId);
        }

        [Fact]
        public void Validate_ShouldFail_WhenDateIsInPast()
        {
            var dto = new MealMenuCreateDto { Date = DateTime.Today.AddDays(-1) };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Date);
        }

        [Fact]
        public void Validate_ShouldFail_WhenFoodItemIdsIsEmpty()
        {
            var dto = new MealMenuCreateDto { FoodItemIds = new List<int>() };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.FoodItemIds);
        }

        [Fact]
        public void Validate_ShouldFail_WhenFoodItemIdsIsNull()
        {
            var dto = new MealMenuCreateDto { FoodItemIds = null! };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.FoodItemIds);
        }
    }
}

