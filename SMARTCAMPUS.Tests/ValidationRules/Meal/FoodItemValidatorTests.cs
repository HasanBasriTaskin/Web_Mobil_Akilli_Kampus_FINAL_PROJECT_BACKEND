using FluentAssertions;
using FluentValidation.TestHelper;
using SMARTCAMPUS.BusinessLayer.ValidationRules.Meal;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.FoodItem;
using SMARTCAMPUS.EntityLayer.Enums;
using Xunit;

namespace SMARTCAMPUS.Tests.ValidationRules.Meal
{
    public class FoodItemCreateValidatorTests
    {
        private readonly FoodItemCreateValidator _validator;

        public FoodItemCreateValidatorTests()
        {
            _validator = new FoodItemCreateValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new FoodItemCreateDto { Name = "Yemek", Category = MealItemCategory.MainCourse };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_ShouldFail_WhenNameIsEmpty(string name)
        {
            var dto = new FoodItemCreateDto { Name = name };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Validate_ShouldFail_WhenCategoryIsInvalid()
        {
            var dto = new FoodItemCreateDto { Name = "Yemek", Category = (MealItemCategory)999 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Category);
        }

        [Fact]
        public void Validate_ShouldFail_WhenCaloriesExceedsMax()
        {
            var dto = new FoodItemCreateDto { Name = "Yemek", Category = MealItemCategory.MainCourse, Calories = 5001 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Calories);
        }
    }

    public class FoodItemUpdateValidatorTests
    {
        private readonly FoodItemUpdateValidator _validator;

        public FoodItemUpdateValidatorTests()
        {
            _validator = new FoodItemUpdateValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new FoodItemUpdateDto { Name = "Yemek" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldFail_WhenCategoryIsInvalid()
        {
            var dto = new FoodItemUpdateDto { Category = (MealItemCategory)999 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Category);
        }

        [Fact]
        public void Validate_ShouldFail_WhenCaloriesIsNegative()
        {
            var dto = new FoodItemUpdateDto { Calories = -1 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Calories);
        }
    }
}

