using FluentAssertions;
using FluentValidation.TestHelper;
using SMARTCAMPUS.BusinessLayer.ValidationRules.Meal;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Cafeteria;
using Xunit;

namespace SMARTCAMPUS.Tests.ValidationRules.Meal
{
    public class CafeteriaCreateValidatorTests
    {
        private readonly CafeteriaCreateValidator _validator;

        public CafeteriaCreateValidatorTests()
        {
            _validator = new CafeteriaCreateValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new CafeteriaCreateDto { Name = "Yemekhane", Location = "Konum", Capacity = 100 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_ShouldFail_WhenNameIsEmpty(string name)
        {
            var dto = new CafeteriaCreateDto { Name = name };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Validate_ShouldFail_WhenCapacityIsInvalid()
        {
            var dto = new CafeteriaCreateDto { Capacity = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Capacity);
        }

        [Fact]
        public void Validate_ShouldFail_WhenCapacityExceedsMax()
        {
            var dto = new CafeteriaCreateDto { Capacity = 5001 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Capacity);
        }
    }

    public class CafeteriaUpdateValidatorTests
    {
        private readonly CafeteriaUpdateValidator _validator;

        public CafeteriaUpdateValidatorTests()
        {
            _validator = new CafeteriaUpdateValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new CafeteriaUpdateDto { Name = "Yemekhane" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldFail_WhenCapacityIsInvalid()
        {
            var dto = new CafeteriaUpdateDto { Capacity = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Capacity);
        }
    }
}

