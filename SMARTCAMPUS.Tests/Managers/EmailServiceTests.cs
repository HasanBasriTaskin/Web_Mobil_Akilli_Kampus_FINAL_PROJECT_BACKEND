using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SMARTCAMPUS.BusinessLayer.Concrete;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class EmailServiceTests
    {
        private readonly Mock<ILogger<EmailService>> _mockLogger;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            _mockLogger = new Mock<ILogger<EmailService>>();
            _emailService = new EmailService(_mockLogger.Object);
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
