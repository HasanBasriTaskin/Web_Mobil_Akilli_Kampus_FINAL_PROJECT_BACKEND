using FluentAssertions;
using SMARTCAMPUS.BusinessLayer.Concrete;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class QRCodeServiceTests
    {
        private readonly QRCodeManager _qrCodeService;

        public QRCodeServiceTests()
        {
            _qrCodeService = new QRCodeManager();
        }

        #region GenerateQRCode Tests

        [Fact]
        public void GenerateQRCode_WithValidInputs_ShouldReturnFormattedString()
        {
            // Arrange
            var prefix = "MEAL";
            var referenceId = 123;

            // Act
            var result = _qrCodeService.GenerateQRCode(prefix, referenceId);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().StartWith($"{prefix}-");
            result.Should().Contain($"-{referenceId}-");
        }

        [Fact]
        public void GenerateQRCode_WithDifferentPrefixes_ShouldReturnDifferentCodes()
        {
            // Arrange
            var referenceId = 456;

            // Act
            var mealCode = _qrCodeService.GenerateQRCode("MEAL", referenceId);
            var eventCode = _qrCodeService.GenerateQRCode("EVENT", referenceId);

            // Assert
            mealCode.Should().StartWith("MEAL-");
            eventCode.Should().StartWith("EVENT-");
            mealCode.Should().NotBe(eventCode);
        }

        [Fact]
        public void GenerateQRCode_WithSameInputs_ShouldGenerateUniqueCodes()
        {
            // Arrange
            var prefix = "MEAL";
            var referenceId = 789;

            // Act
            var code1 = _qrCodeService.GenerateQRCode(prefix, referenceId);
            var code2 = _qrCodeService.GenerateQRCode(prefix, referenceId);

            // Assert
            code1.Should().NotBe(code2); // Should be unique due to timestamp and random part
        }

        [Fact]
        public void GenerateQRCode_ShouldContainReferenceId()
        {
            // Arrange
            var prefix = "MEAL";
            var referenceId = 999;

            // Act
            var result = _qrCodeService.GenerateQRCode(prefix, referenceId);

            // Assert
            result.Should().Contain($"-{referenceId}-");
        }

        #endregion

        #region ValidateQRCodeFormat Tests

        [Fact]
        public void ValidateQRCodeFormat_WithValidMealQRCode_ShouldReturnTrue()
        {
            // Arrange
            var qrCode = "MEAL-123-ABC123-DEF456";
            var expectedPrefix = "MEAL";

            // Act
            var result = _qrCodeService.ValidateQRCodeFormat(qrCode, expectedPrefix);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateQRCodeFormat_WithValidEventQRCode_ShouldReturnTrue()
        {
            // Arrange
            var qrCode = "EVENT-456-XYZ789-ABC123";
            var expectedPrefix = "EVENT";

            // Act
            var result = _qrCodeService.ValidateQRCodeFormat(qrCode, expectedPrefix);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateQRCodeFormat_WithWrongPrefix_ShouldReturnFalse()
        {
            // Arrange
            var qrCode = "MEAL-123-ABC123-DEF456";
            var expectedPrefix = "EVENT"; // Wrong prefix

            // Act
            var result = _qrCodeService.ValidateQRCodeFormat(qrCode, expectedPrefix);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateQRCodeFormat_WithCaseInsensitivePrefix_ShouldReturnTrue()
        {
            // Arrange
            var qrCode = "meal-123-ABC123-DEF456"; // Lowercase prefix
            var expectedPrefix = "MEAL"; // Uppercase expected

            // Act
            var result = _qrCodeService.ValidateQRCodeFormat(qrCode, expectedPrefix);

            // Assert
            result.Should().BeTrue(); // Should be case-insensitive
        }

        [Fact]
        public void ValidateQRCodeFormat_WithNullQRCode_ShouldReturnFalse()
        {
            // Arrange
            string? qrCode = null;
            var expectedPrefix = "MEAL";

            // Act
            var result = _qrCodeService.ValidateQRCodeFormat(qrCode!, expectedPrefix);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateQRCodeFormat_WithEmptyQRCode_ShouldReturnFalse()
        {
            // Arrange
            var qrCode = "";
            var expectedPrefix = "MEAL";

            // Act
            var result = _qrCodeService.ValidateQRCodeFormat(qrCode, expectedPrefix);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateQRCodeFormat_WithInvalidFormat_ShouldReturnFalse()
        {
            // Arrange
            var qrCode = "INVALID_FORMAT"; // No separator
            var expectedPrefix = "MEAL";

            // Act
            var result = _qrCodeService.ValidateQRCodeFormat(qrCode, expectedPrefix);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region ExtractReferenceId Tests

        [Fact]
        public void ExtractReferenceId_WithValidQRCode_ShouldReturnReferenceId()
        {
            // Arrange
            var qrCode = "MEAL-123-ABC123-DEF456";
            var expectedId = 123;

            // Act
            var result = _qrCodeService.ExtractReferenceId(qrCode);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedId);
        }

        [Fact]
        public void ExtractReferenceId_WithDifferentReferenceId_ShouldReturnCorrectId()
        {
            // Arrange
            var qrCode = "EVENT-999-XYZ789-ABC123";
            var expectedId = 999;

            // Act
            var result = _qrCodeService.ExtractReferenceId(qrCode);

            // Assert
            result.Should().Be(expectedId);
        }

        [Fact]
        public void ExtractReferenceId_WithNullQRCode_ShouldReturnNull()
        {
            // Arrange
            string? qrCode = null;

            // Act
            var result = _qrCodeService.ExtractReferenceId(qrCode!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ExtractReferenceId_WithEmptyQRCode_ShouldReturnNull()
        {
            // Arrange
            var qrCode = "";

            // Act
            var result = _qrCodeService.ExtractReferenceId(qrCode);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ExtractReferenceId_WithInvalidFormat_ShouldReturnNull()
        {
            // Arrange
            var qrCode = "INVALID_FORMAT";

            // Act
            var result = _qrCodeService.ExtractReferenceId(qrCode);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ExtractReferenceId_WithNonNumericReferenceId_ShouldReturnNull()
        {
            // Arrange
            var qrCode = "MEAL-ABC-123-DEF456"; // ABC is not numeric

            // Act
            var result = _qrCodeService.ExtractReferenceId(qrCode);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GenerateQRCodeImage Tests

        [Fact]
        public void GenerateQRCodeImage_WithValidQRCode_ShouldReturnByteArray()
        {
            // Arrange
            var qrCode = "MEAL-123-ABC123-DEF456";

            // Act
            var result = _qrCodeService.GenerateQRCodeImage(qrCode);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GenerateQRCodeImage_WithDifferentQRCodes_ShouldReturnDifferentImages()
        {
            // Arrange
            var qrCode1 = "MEAL-123-ABC123-DEF456";
            var qrCode2 = "EVENT-456-XYZ789-ABC123";

            // Act
            var image1 = _qrCodeService.GenerateQRCodeImage(qrCode1);
            var image2 = _qrCodeService.GenerateQRCodeImage(qrCode2);

            // Assert
            image1.Should().NotBeEquivalentTo(image2);
        }

        [Fact]
        public void GenerateQRCodeImage_ShouldReturnPNGFormat()
        {
            // Arrange
            var qrCode = "MEAL-123-ABC123-DEF456";

            // Act
            var result = _qrCodeService.GenerateQRCodeImage(qrCode);

            // Assert
            result.Should().NotBeNull();
            // PNG files start with specific bytes: 89 50 4E 47 (PNG signature)
            result[0].Should().Be(0x89);
            result[1].Should().Be(0x50);
            result[2].Should().Be(0x4E);
            result[3].Should().Be(0x47);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void GenerateAndValidateQRCode_ShouldWorkTogether()
        {
            // Arrange
            var prefix = "MEAL";
            var referenceId = 555;

            // Act
            var qrCode = _qrCodeService.GenerateQRCode(prefix, referenceId);
            var isValid = _qrCodeService.ValidateQRCodeFormat(qrCode, prefix);
            var extractedId = _qrCodeService.ExtractReferenceId(qrCode);

            // Assert
            isValid.Should().BeTrue();
            extractedId.Should().Be(referenceId);
        }

        [Fact]
        public void GenerateValidateAndExtract_ShouldWorkEndToEnd()
        {
            // Arrange
            var prefix = "EVENT";
            var referenceId = 777;

            // Act
            var qrCode = _qrCodeService.GenerateQRCode(prefix, referenceId);
            var isValid = _qrCodeService.ValidateQRCodeFormat(qrCode, prefix);
            var extractedId = _qrCodeService.ExtractReferenceId(qrCode);
            var image = _qrCodeService.GenerateQRCodeImage(qrCode);

            // Assert
            isValid.Should().BeTrue();
            extractedId.Should().Be(referenceId);
            image.Should().NotBeNull();
            image.Length.Should().BeGreaterThan(0);
        }

        #endregion
    }
}

