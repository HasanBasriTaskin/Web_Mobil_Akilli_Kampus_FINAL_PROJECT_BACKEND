using FluentAssertions;
using FluentValidation.TestHelper;
using SMARTCAMPUS.BusinessLayer.ValidationRules.Event;
using SMARTCAMPUS.EntityLayer.DTOs.Event;
using Xunit;

namespace SMARTCAMPUS.Tests.ValidationRules.Event
{
    public class EventCreateValidatorTests
    {
        private readonly EventCreateValidator _validator;

        public EventCreateValidatorTests()
        {
            _validator = new EventCreateValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new EventCreateDto
            {
                Title = "Etkinlik",
                Description = "Açıklama",
                CategoryId = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Konum",
                Capacity = 100,
                Price = 50
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_ShouldFail_WhenTitleIsEmpty(string title)
        {
            var dto = new EventCreateDto { Title = title };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Validate_ShouldFail_WhenCategoryIdIsInvalid()
        {
            var dto = new EventCreateDto { CategoryId = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        }

        [Fact]
        public void Validate_ShouldFail_WhenStartDateIsInPast()
        {
            var dto = new EventCreateDto { StartDate = DateTime.UtcNow.AddDays(-1) };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.StartDate);
        }

        [Fact]
        public void Validate_ShouldFail_WhenEndDateIsBeforeStartDate()
        {
            var dto = new EventCreateDto
            {
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(1)
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.EndDate);
        }

        [Fact]
        public void Validate_ShouldFail_WhenCapacityIsInvalid()
        {
            var dto = new EventCreateDto { Capacity = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Capacity);
        }
    }

    public class EventUpdateValidatorTests
    {
        private readonly EventUpdateValidator _validator;

        public EventUpdateValidatorTests()
        {
            _validator = new EventUpdateValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new EventUpdateDto
            {
                Title = "Etkinlik",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldFail_WhenEndDateIsBeforeStartDate()
        {
            var dto = new EventUpdateDto
            {
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(1)
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.EndDate);
        }

        [Fact]
        public void Validate_ShouldFail_WhenCapacityIsInvalid()
        {
            var dto = new EventUpdateDto { Capacity = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Capacity);
        }

        [Fact]
        public void Validate_ShouldFail_WhenPriceIsNegative()
        {
            var dto = new EventUpdateDto { Price = -1 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Price);
        }
    }
}

