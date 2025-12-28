using FluentAssertions;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.EntityLayer.DTOs.Wallet;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class PaymentServiceTests
    {
        private readonly MockPaymentManager _paymentService;

        public PaymentServiceTests()
        {
            _paymentService = new MockPaymentManager();
        }

        #region Successful Payment Tests

        [Fact]
        public void ProcessPayment_WithValidTestCard_ShouldReturnSuccess()
        {
            // Arrange
            var paymentDto = new PaymentDto
            {
                CardNumber = "1234-5678-1234-5678",
                CVV = "123",
                ExpiryDate = "01/26",
                Amount = 100.00m
            };

            // Act
            var result = _paymentService.ProcessPayment(paymentDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.TransactionId.Should().NotBeNullOrEmpty();
            result.TransactionId.Should().StartWith("TXN-");
            result.ErrorCode.Should().BeNull();
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void ProcessPayment_WithValidTestCard_WithoutDashes_ShouldReturnSuccess()
        {
            // Arrange
            var paymentDto = new PaymentDto
            {
                CardNumber = "1234567812345678", // Without dashes
                CVV = "123",
                ExpiryDate = "01/26",
                Amount = 50.00m
            };

            // Act
            var result = _paymentService.ProcessPayment(paymentDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.TransactionId.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region Insufficient Funds Tests

        [Fact]
        public void ProcessPayment_WithInsufficientFundsCard_ShouldReturnError()
        {
            // Arrange
            var paymentDto = new PaymentDto
            {
                CardNumber = "0000-0000-0000-0000",
                CVV = "000",
                ExpiryDate = "01/26",
                Amount = 100.00m
            };

            // Act
            var result = _paymentService.ProcessPayment(paymentDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorCode.Should().Be("INSUFFICIENT_FUNDS");
            result.ErrorMessage.Should().Be("Yetersiz bakiye");
            result.TransactionId.Should().BeNull();
        }

        #endregion

        #region Blocked Card Tests

        [Fact]
        public void ProcessPayment_WithBlockedCard_ShouldReturnError()
        {
            // Arrange
            var paymentDto = new PaymentDto
            {
                CardNumber = "9999-9999-9999-9999",
                CVV = "999",
                ExpiryDate = "01/26",
                Amount = 100.00m
            };

            // Act
            var result = _paymentService.ProcessPayment(paymentDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorCode.Should().Be("CARD_BLOCKED");
            result.ErrorMessage.Should().Be("Kart bloke edilmiş");
            result.TransactionId.Should().BeNull();
        }

        #endregion

        #region Expired Card Tests

        [Fact]
        public void ProcessPayment_WithExpiredCard_ShouldReturnError()
        {
            // Arrange
            var paymentDto = new PaymentDto
            {
                CardNumber = "1111-1111-1111-1111",
                CVV = "111",
                ExpiryDate = "01/26",
                Amount = 100.00m
            };

            // Act
            var result = _paymentService.ProcessPayment(paymentDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorCode.Should().Be("CARD_EXPIRED");
            result.ErrorMessage.Should().Be("Kart süresi dolmuş");
            result.TransactionId.Should().BeNull();
        }

        #endregion

        #region Invalid Card Tests

        [Fact]
        public void ProcessPayment_WithInvalidCardNumber_ShouldReturnError()
        {
            // Arrange
            var paymentDto = new PaymentDto
            {
                CardNumber = "9999-8888-7777-6666", // Invalid test card
                CVV = "123",
                ExpiryDate = "01/26",
                Amount = 100.00m
            };

            // Act
            var result = _paymentService.ProcessPayment(paymentDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorCode.Should().Be("INVALID_CARD");
            result.ErrorMessage.Should().Be("Geçersiz kart numarası");
            result.TransactionId.Should().BeNull();
        }

        [Fact]
        public void ProcessPayment_WithInvalidCVV_ShouldReturnError()
        {
            // Arrange
            var paymentDto = new PaymentDto
            {
                CardNumber = "1234-5678-1234-5678",
                CVV = "999", // Wrong CVV
                ExpiryDate = "01/26",
                Amount = 100.00m
            };

            // Act
            var result = _paymentService.ProcessPayment(paymentDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorCode.Should().Be("INVALID_CVV");
            result.ErrorMessage.Should().Be("Geçersiz güvenlik kodu (CVV)");
            result.TransactionId.Should().BeNull();
        }

        [Fact]
        public void ProcessPayment_WithInvalidExpiryDate_ShouldReturnError()
        {
            // Arrange
            var paymentDto = new PaymentDto
            {
                CardNumber = "1234-5678-1234-5678",
                CVV = "123",
                ExpiryDate = "12/25", // Wrong expiry date
                Amount = 100.00m
            };

            // Act
            var result = _paymentService.ProcessPayment(paymentDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorCode.Should().Be("INVALID_EXPIRY");
            result.ErrorMessage.Should().Be("Geçersiz son kullanma tarihi");
            result.TransactionId.Should().BeNull();
        }

        #endregion

        #region Invalid Amount Tests

        [Fact]
        public void ProcessPayment_WithZeroAmount_ShouldReturnError()
        {
            // Arrange
            var paymentDto = new PaymentDto
            {
                CardNumber = "1234-5678-1234-5678",
                CVV = "123",
                ExpiryDate = "01/26",
                Amount = 0m
            };

            // Act
            var result = _paymentService.ProcessPayment(paymentDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorCode.Should().Be("INVALID_AMOUNT");
            result.ErrorMessage.Should().Be("Geçersiz tutar");
            result.TransactionId.Should().BeNull();
        }

        [Fact]
        public void ProcessPayment_WithNegativeAmount_ShouldReturnError()
        {
            // Arrange
            var paymentDto = new PaymentDto
            {
                CardNumber = "1234-5678-1234-5678",
                CVV = "123",
                ExpiryDate = "01/26",
                Amount = -50.00m
            };

            // Act
            var result = _paymentService.ProcessPayment(paymentDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorCode.Should().Be("INVALID_AMOUNT");
            result.ErrorMessage.Should().Be("Geçersiz tutar");
            result.TransactionId.Should().BeNull();
        }

        #endregion

        #region Transaction ID Format Tests

        [Fact]
        public void ProcessPayment_WithValidCard_ShouldGenerateUniqueTransactionIds()
        {
            // Arrange
            var paymentDto = new PaymentDto
            {
                CardNumber = "1234-5678-1234-5678",
                CVV = "123",
                ExpiryDate = "01/26",
                Amount = 100.00m
            };

            // Act
            var result1 = _paymentService.ProcessPayment(paymentDto);
            var result2 = _paymentService.ProcessPayment(paymentDto);

            // Assert
            result1.TransactionId.Should().NotBe(result2.TransactionId);
            result1.TransactionId.Should().StartWith("TXN-");
            result2.TransactionId.Should().StartWith("TXN-");
        }

        #endregion
    }
}

