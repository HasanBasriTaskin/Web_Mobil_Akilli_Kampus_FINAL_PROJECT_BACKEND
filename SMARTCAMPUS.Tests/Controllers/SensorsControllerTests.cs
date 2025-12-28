using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class SensorsControllerTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly SensorsController _controller;

        public SensorsControllerTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new CampusContext(options);
            _controller = new SensorsController(_context);
            SetupHttpContext("user1");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SetupHttpContext(string userId)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetAllSensors_ShouldReturnOk()
        {
            var sensor = new Sensor
            {
                Id = 1,
                SensorId = "TEMP-01",
                Name = "Temperature Sensor",
                Type = SensorType.Temperature,
                Location = "Building A",
                IsOnline = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            _context.Sensors.Add(sensor);
            await _context.SaveChangesAsync();

            var result = await _controller.GetAllSensors();

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var sensors = (List<SMARTCAMPUS.EntityLayer.DTOs.Sensors.SensorDto>)okResult.Value!;
            sensors.Should().HaveCount(1);
            sensors[0].SensorId.Should().Be("TEMP-01");
        }

        [Fact]
        public async Task GetAllSensors_ShouldFilterInactive()
        {
            var activeSensor = new Sensor
            {
                Id = 1,
                SensorId = "TEMP-01",
                Name = "Active",
                Type = SensorType.Temperature,
                Location = "A",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            var inactiveSensor = new Sensor
            {
                Id = 2,
                SensorId = "TEMP-02",
                Name = "Inactive",
                Type = SensorType.Temperature,
                Location = "B",
                IsActive = false,
                CreatedDate = DateTime.UtcNow
            };
            _context.Sensors.AddRange(activeSensor, inactiveSensor);
            await _context.SaveChangesAsync();

            var result = await _controller.GetAllSensors();

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var sensors = (List<SMARTCAMPUS.EntityLayer.DTOs.Sensors.SensorDto>)okResult.Value!;
            sensors.Should().HaveCount(1);
            sensors[0].SensorId.Should().Be("TEMP-01");
        }

        [Fact]
        public async Task GetDashboard_ShouldReturnOk()
        {
            var sensor = new Sensor
            {
                Id = 1,
                SensorId = "TEMP-01",
                Name = "Temperature",
                Type = SensorType.Temperature,
                Location = "A",
                IsOnline = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            var reading = new SensorReading
            {
                Id = 1,
                SensorId = 1,
                Sensor = sensor,
                Value = 25.5,
                Unit = "°C",
                Timestamp = DateTime.UtcNow,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            _context.Sensors.Add(sensor);
            _context.SensorReadings.Add(reading);
            await _context.SaveChangesAsync();

            var result = await _controller.GetDashboard();

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var dashboard = (SMARTCAMPUS.EntityLayer.DTOs.Sensors.SensorDashboardDto)okResult.Value!;
            dashboard.TotalSensors.Should().Be(1);
            dashboard.OnlineSensors.Should().Be(1);
        }

        [Fact]
        public async Task GetDashboard_ShouldCalculateEnvironmentStats()
        {
            var tempSensor = new Sensor
            {
                Id = 1,
                SensorId = "TEMP-01",
                Name = "Temp",
                Type = SensorType.Temperature,
                Location = "A",
                IsOnline = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            var occSensor = new Sensor
            {
                Id = 2,
                SensorId = "OCC-01",
                Name = "Occupancy",
                Type = SensorType.Occupancy,
                Location = "B",
                IsOnline = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            var tempReading = new SensorReading
            {
                Id = 1,
                SensorId = 1,
                Sensor = tempSensor,
                Value = 22.0,
                Unit = "°C",
                Timestamp = DateTime.UtcNow,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            var occReading = new SensorReading
            {
                Id = 2,
                SensorId = 2,
                Sensor = occSensor,
                Value = 75.0,
                Unit = "%",
                Timestamp = DateTime.UtcNow,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            _context.Sensors.AddRange(tempSensor, occSensor);
            _context.SensorReadings.AddRange(tempReading, occReading);
            await _context.SaveChangesAsync();

            var result = await _controller.GetDashboard();

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var dashboard = (SMARTCAMPUS.EntityLayer.DTOs.Sensors.SensorDashboardDto)okResult.Value!;
            dashboard.Environment.AverageTemperature.Should().Be(22.0);
            dashboard.Environment.AverageOccupancy.Should().Be(75.0);
        }

        [Fact]
        public async Task GetSensorReadings_ShouldReturnOk()
        {
            var sensor = new Sensor
            {
                Id = 1,
                SensorId = "TEMP-01",
                Name = "Temperature",
                Type = SensorType.Temperature,
                Location = "A",
                IsOnline = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            var reading1 = new SensorReading
            {
                Id = 1,
                SensorId = 1,
                Sensor = sensor,
                Value = 25.0,
                Unit = "°C",
                Timestamp = DateTime.UtcNow.AddMinutes(-10),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            var reading2 = new SensorReading
            {
                Id = 2,
                SensorId = 1,
                Sensor = sensor,
                Value = 26.0,
                Unit = "°C",
                Timestamp = DateTime.UtcNow,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            _context.Sensors.Add(sensor);
            _context.SensorReadings.AddRange(reading1, reading2);
            await _context.SaveChangesAsync();

            var result = await _controller.GetSensorReadings("TEMP-01");

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var readings = (List<SMARTCAMPUS.EntityLayer.DTOs.Sensors.SensorReadingDto>)okResult.Value!;
            readings.Should().HaveCount(2);
            readings[0].Value.Should().Be(26.0);
        }

        [Fact]
        public async Task GetSensorReadings_ShouldReturnNotFound_WhenSensorNotFound()
        {
            var result = await _controller.GetSensorReadings("NONEXISTENT");

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetSensorReadings_ShouldRespectLimit()
        {
            var sensor = new Sensor
            {
                Id = 1,
                SensorId = "TEMP-01",
                Name = "Temperature",
                Type = SensorType.Temperature,
                Location = "A",
                IsOnline = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            var readings = Enumerable.Range(1, 100).Select(i => new SensorReading
            {
                Id = i,
                SensorId = 1,
                Sensor = sensor,
                Value = 25.0 + i,
                Unit = "°C",
                Timestamp = DateTime.UtcNow.AddMinutes(-i),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }).ToList();
            _context.Sensors.Add(sensor);
            _context.SensorReadings.AddRange(readings);
            await _context.SaveChangesAsync();

            var result = await _controller.GetSensorReadings("TEMP-01", limit: 10);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var resultReadings = (List<SMARTCAMPUS.EntityLayer.DTOs.Sensors.SensorReadingDto>)okResult.Value!;
            resultReadings.Should().HaveCount(10);
        }
    }
}

