using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SMARTCAMPUS.API.BackgroundServices;
using SMARTCAMPUS.API.Hubs;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using System.Reflection;
using Xunit;

namespace SMARTCAMPUS.Tests.BackgroundServices
{
    public class SensorSimulationServiceTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
        private readonly Mock<ILogger<SensorSimulationService>> _mockLogger;
        private readonly IServiceProvider _serviceProvider;
        private readonly SensorSimulationService _service;

        public SensorSimulationServiceTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _mockHubContext = new Mock<IHubContext<NotificationHub>>();
            _mockLogger = new Mock<ILogger<SensorSimulationService>>();

            var mockClients = new Mock<IHubClients>();
            var mockAll = new Mock<IClientProxy>();
            mockClients.Setup(x => x.All).Returns(mockAll.Object);
            _mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

            var services = new ServiceCollection();
            services.AddSingleton(_context);
            _serviceProvider = services.BuildServiceProvider();

            _service = new SensorSimulationService(_serviceProvider, _mockHubContext.Object, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void Constructor_ShouldInitialize()
        {
            _service.Should().NotBeNull();
        }

        [Fact]
        public async Task EnsureSensorsExistAsync_ShouldCreateSensors_WhenNoSensorsExist()
        {
            // Act
            var method = typeof(SensorSimulationService).GetMethod("EnsureSensorsExistAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method!.Invoke(_service, null)!;

            // Assert
            var sensors = await _context.Sensors.ToListAsync();
            sensors.Should().HaveCount(8);
            sensors.Should().Contain(s => s.SensorId == "TEMP-01");
            sensors.Should().Contain(s => s.SensorId == "OCC-01");
        }

        [Fact]
        public async Task EnsureSensorsExistAsync_ShouldNotCreateSensors_WhenSensorsAlreadyExist()
        {
            // Arrange
            var existingSensor = new Sensor
            {
                SensorId = "EXISTING-01",
                Name = "Existing Sensor",
                Type = SensorType.Temperature,
                Location = "Test",
                IsOnline = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            _context.Sensors.Add(existingSensor);
            await _context.SaveChangesAsync();

            // Act
            var method = typeof(SensorSimulationService).GetMethod("EnsureSensorsExistAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method!.Invoke(_service, null)!;

            // Assert
            var sensors = await _context.Sensors.ToListAsync();
            sensors.Should().HaveCount(1);
            sensors.Should().Contain(s => s.SensorId == "EXISTING-01");
        }

        [Fact]
        public async Task GenerateSensorReadingsAsync_ShouldGenerateReadings_ForAllActiveSensors()
        {
            // Arrange
            var sensor = new Sensor
            {
                SensorId = "TEMP-01",
                Name = "Test Sensor",
                Type = SensorType.Temperature,
                Location = "Test",
                IsOnline = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            _context.Sensors.Add(sensor);
            await _context.SaveChangesAsync();

            // Act
            var method = typeof(SensorSimulationService).GetMethod("GenerateSensorReadingsAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = await (Task<SMARTCAMPUS.EntityLayer.DTOs.Sensors.SensorDashboardDto>)method!.Invoke(_service, null)!;

            // Assert
            result.Should().NotBeNull();
            result.TotalSensors.Should().Be(1);
            result.OnlineSensors.Should().Be(1);
            result.LatestReadings.Should().HaveCount(1);
            result.LatestReadings.First().SensorId.Should().Be("TEMP-01");
            result.LatestReadings.First().Value.Should().BeInRange(18, 28);
            result.LatestReadings.First().Unit.Should().Be("°C");

            var readings = await _context.SensorReadings.ToListAsync();
            readings.Should().HaveCount(1);
        }

        [Fact]
        public async Task GenerateSensorReadingsAsync_ShouldGenerateCorrectValues_ForDifferentSensorTypes()
        {
            // Arrange
            var sensors = new List<Sensor>
            {
                new() { SensorId = "TEMP-01", Name = "Temp", Type = SensorType.Temperature, Location = "Test", IsOnline = true, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { SensorId = "HUM-01", Name = "Humidity", Type = SensorType.Humidity, Location = "Test", IsOnline = true, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { SensorId = "OCC-01", Name = "Occupancy", Type = SensorType.Occupancy, Location = "Test", IsOnline = true, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { SensorId = "LIGHT-01", Name = "Light", Type = SensorType.Light, Location = "Test", IsOnline = true, IsActive = true, CreatedDate = DateTime.UtcNow }
            };
            _context.Sensors.AddRange(sensors);
            await _context.SaveChangesAsync();

            // Act
            var method = typeof(SensorSimulationService).GetMethod("GenerateSensorReadingsAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = await (Task<SMARTCAMPUS.EntityLayer.DTOs.Sensors.SensorDashboardDto>)method!.Invoke(_service, null)!;

            // Assert
            result.LatestReadings.Should().HaveCount(4);
            result.LatestReadings.Should().Contain(r => r.SensorId == "TEMP-01" && r.Unit == "°C" && r.Value >= 18 && r.Value <= 28);
            result.LatestReadings.Should().Contain(r => r.SensorId == "HUM-01" && r.Unit == "%" && r.Value >= 40 && r.Value <= 70);
            result.LatestReadings.Should().Contain(r => r.SensorId == "OCC-01" && r.Unit == "%" && r.Value >= 0 && r.Value <= 100);
            result.LatestReadings.Should().Contain(r => r.SensorId == "LIGHT-01" && r.Unit == "lux" && r.Value >= 200 && r.Value <= 800);
        }
    }
}

