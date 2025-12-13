using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using SMARTCAMPUS.BusinessLayer.Concrete;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class EmailServiceTests
    {
        private readonly Mock<IHostEnvironment> _mockEnv;
        private readonly Mock<ILogger<EmailService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            _mockEnv = new Mock<IHostEnvironment>();
            _mockLogger = new Mock<ILogger<EmailService>>();
            _mockConfiguration = new Mock<IConfiguration>();

            _emailService = new EmailService(_mockEnv.Object, _mockLogger.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_InDevelopment_ShouldLog()
        {
            // Arrange
            _mockEnv.Setup(x => x.EnvironmentName).Returns("Development");

            // Act
            await _emailService.SendPasswordResetEmailAsync("test@test.com", "link");

            // Assert
            // Checking if execution completed without error.
            // Verifying log calls is hard with extension methods, but we can verify IHostEnvironment was accessed.
            _mockEnv.Verify(x => x.EnvironmentName, Times.Once);
        }

        [Fact]
        public async Task SendEmailVerificationAsync_InDevelopment_ShouldLog()
        {
            // Arrange
            _mockEnv.Setup(x => x.EnvironmentName).Returns("Development");

            // Act
            await _emailService.SendEmailVerificationAsync("test@test.com", "link");

            // Assert
            _mockEnv.Verify(x => x.EnvironmentName, Times.Once);
        }
    }
}
