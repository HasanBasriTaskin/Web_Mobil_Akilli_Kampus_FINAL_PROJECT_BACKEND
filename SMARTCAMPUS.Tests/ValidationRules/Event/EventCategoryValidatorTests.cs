using FluentAssertions;
using FluentValidation.TestHelper;
using SMARTCAMPUS.BusinessLayer.ValidationRules.Event;
using SMARTCAMPUS.EntityLayer.DTOs.Event;
using Xunit;

namespace SMARTCAMPUS.Tests.ValidationRules.Event
{
    public class EventCategoryCreateValidatorTests
    {
        private readonly EventCategoryCreateValidator _validator;

        public EventCategoryCreateValidatorTests()
        {
            _validator = new EventCategoryCreateValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new EventCategoryCreateDto { Name = "Kategori" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_ShouldFail_WhenNameIsEmpty(string name)
        {
            var dto = new EventCategoryCreateDto { Name = name };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Validate_ShouldFail_WhenNameExceedsMaxLength()
        {
            var dto = new EventCategoryCreateDto { Name = new string('A', 101) };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Validate_ShouldFail_WhenDescriptionExceedsMaxLength()
        {
            var dto = new EventCategoryCreateDto { Name = "Kategori", Description = new string('A', 501) };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Validate_ShouldFail_WhenIconNameExceedsMaxLength()
        {
            var dto = new EventCategoryCreateDto { Name = "Kategori", IconName = new string('A', 51) };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.IconName);
        }
    }

    public class EventCategoryUpdateValidatorTests
    {
        private readonly EventCategoryUpdateValidator _validator;

        public EventCategoryUpdateValidatorTests()
        {
            _validator = new EventCategoryUpdateValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new EventCategoryUpdateDto { Name = "Kategori" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldFail_WhenNameExceedsMaxLength()
        {
            var dto = new EventCategoryUpdateDto { Name = new string('A', 101) };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Validate_ShouldFail_WhenDescriptionExceedsMaxLength()
        {
            var dto = new EventCategoryUpdateDto { Description = new string('A', 501) };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
    }
}

