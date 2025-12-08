using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SMARTCAMPUS.API.Controllers;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class WeatherForecastControllerTests
    {
        private readonly Mock<ILogger<WeatherForecastController>> _mockLogger;
        private readonly WeatherForecastController _controller;

        public WeatherForecastControllerTests()
        {
            _mockLogger = new Mock<ILogger<WeatherForecastController>>();
            _controller = new WeatherForecastController(_mockLogger.Object);
        }

        [Fact]
        public void Get_ShouldReturnForecasts()
        {
            // Act
            var result = _controller.Get();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
        }
    }
}
