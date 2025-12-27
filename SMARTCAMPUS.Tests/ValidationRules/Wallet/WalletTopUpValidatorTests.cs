using FluentAssertions;
using FluentValidation.TestHelper;
using SMARTCAMPUS.BusinessLayer.ValidationRules.Wallet;
using SMARTCAMPUS.EntityLayer.DTOs.Wallet;
using Xunit;

namespace SMARTCAMPUS.Tests.ValidationRules.Wallet
{
    public class WalletTopUpValidatorTests
    {
        private readonly WalletTopUpValidator _validator;

        public WalletTopUpValidatorTests()
        {
            _validator = new WalletTopUpValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDtoIsValid()
        {
            var dto = new WalletTopUpDto
            {
                CardNumber = "1234567890123456",
                CVV = "123",
                ExpiryDate = "12/25",
                Amount = 100
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("123456789012345")] // 15 digits
        [InlineData("12345678901234567")] // 17 digits
        [InlineData("123456789012345a")] // contains letter
        public void Validate_ShouldFail_WhenCardNumberIsInvalid(string cardNumber)
        {
            var dto = new WalletTopUpDto
            {
                CardNumber = cardNumber,
                CVV = "123",
                ExpiryDate = "12/25",
                Amount = 100
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.CardNumber);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("12")] // 2 digits
        [InlineData("12345")] // 5 digits
        [InlineData("12a")] // contains letter
        public void Validate_ShouldFail_WhenCVVIsInvalid(string cvv)
        {
            var dto = new WalletTopUpDto
            {
                CardNumber = "1234567890123456",
                CVV = cvv,
                ExpiryDate = "12/25",
                Amount = 100
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.CVV);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("1/25")] // invalid format
        [InlineData("13/25")] // invalid month
        [InlineData("00/25")] // invalid month
        [InlineData("12/5")] // invalid year format
        [InlineData("12/2025")] // invalid year format
        public void Validate_ShouldFail_WhenExpiryDateIsInvalid(string expiryDate)
        {
            var dto = new WalletTopUpDto
            {
                CardNumber = "1234567890123456",
                CVV = "123",
                ExpiryDate = expiryDate,
                Amount = 100
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.ExpiryDate);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(10001)] // exceeds max
        public void Validate_ShouldFail_WhenAmountIsInvalid(decimal amount)
        {
            var dto = new WalletTopUpDto
            {
                CardNumber = "1234567890123456",
                CVV = "123",
                ExpiryDate = "12/25",
                Amount = amount
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void Validate_ShouldAccept_WhenCVVIs4Digits()
        {
            var dto = new WalletTopUpDto
            {
                CardNumber = "1234567890123456",
                CVV = "1234",
                ExpiryDate = "12/25",
                Amount = 100
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldAccept_WhenAmountIsMaximum()
        {
            var dto = new WalletTopUpDto
            {
                CardNumber = "1234567890123456",
                CVV = "123",
                ExpiryDate = "12/25",
                Amount = 10000
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }
    }
}

