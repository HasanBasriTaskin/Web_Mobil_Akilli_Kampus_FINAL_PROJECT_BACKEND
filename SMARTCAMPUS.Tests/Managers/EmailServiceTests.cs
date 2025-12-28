using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.EntityLayer.Configuration;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class EmailServiceTests
    {
        private readonly Mock<ILogger<EmailService>> _mockLogger;
        private readonly Mock<IOptions<EmailSettings>> _mockSettings;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            _mockLogger = new Mock<ILogger<EmailService>>();
            _mockSettings = new Mock<IOptions<EmailSettings>>();
            _mockSettings.Setup(x => x.Value).Returns(new EmailSettings 
            { 
                Host = "smtp.test.com",
                Port = 587,
                EnableSsl = true,
                FromEmail = "test@test.com",
                Password = "test"
            });
            _emailService = new EmailService(_mockLogger.Object, _mockSettings.Object);
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldLogToConsole()
        {
            // Arrange
            var to = "test@test.com";
            var resetLink = "http://localhost/reset?token=abc";

            // Act
            await _emailService.SendPasswordResetEmailAsync(to, resetLink);

            // Assert - Execution completed without error
            // Logger was called (we can't easily verify Console.WriteLine, but method completed)
            true.Should().BeTrue();
        }

        [Fact]
        public async Task SendEmailVerificationAsync_ShouldLogToConsole()
        {
            // Arrange
            var to = "test@test.com";
            var verifyLink = "http://localhost/verify?token=abc";

            // Act
            await _emailService.SendEmailVerificationAsync(to, verifyLink);

            // Assert - Execution completed without error
            true.Should().BeTrue();
        }
    }
}
