using FluentAssertions;
using SMARTCAMPUS.API;
using Xunit;

namespace SMARTCAMPUS.Tests.Entities
{
    public class WeatherForecastTests
    {
        [Fact]
        public void WeatherForecast_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var date = DateOnly.FromDateTime(DateTime.Now);
            var forecast = new WeatherForecast
            {
                Date = date,
                TemperatureC = 25,
                Summary = "Hot"
            };

            // Act & Assert
            forecast.Date.Should().Be(date);
            forecast.TemperatureC.Should().Be(25);
            forecast.Summary.Should().Be("Hot");
            forecast.TemperatureF.Should().Be(76); // 32 + (int)(25 / 0.5556) => 32 + 44.99 => 76 (approx) or formula check
        }
    }
}
