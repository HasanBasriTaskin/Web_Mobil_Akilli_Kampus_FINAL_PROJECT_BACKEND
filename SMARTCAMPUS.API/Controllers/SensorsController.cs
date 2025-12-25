using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs.Sensors;

namespace SMARTCAMPUS.API.Controllers
{
    /// <summary>
    /// IoT Sensör verileri endpoint'leri
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class SensorsController : ControllerBase
    {
        private readonly CampusContext _context;

        public SensorsController(CampusContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm sensörleri listeler
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllSensors()
        {
            var sensors = await _context.Sensors
                .Where(s => s.IsActive)
                .Select(s => new SensorDto
                {
                    Id = s.Id,
                    SensorId = s.SensorId,
                    Name = s.Name,
                    Type = s.Type.ToString(),
                    Location = s.Location,
                    IsOnline = s.IsOnline,
                    LastReading = s.LastReading
                })
                .ToListAsync();

            return Ok(sensors);
        }

        /// <summary>
        /// Sensör dashboard özeti
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var sensors = await _context.Sensors.Where(s => s.IsActive).ToListAsync();

            // Son okumaları al
            var latestReadings = await _context.SensorReadings
                .Include(r => r.Sensor)
                .Where(r => r.IsActive && r.Sensor!.IsActive)
                .GroupBy(r => r.SensorId)
                .Select(g => g.OrderByDescending(r => r.Timestamp).First())
                .ToListAsync();

            var temperatures = latestReadings
                .Where(r => r.Sensor?.Type == EntityLayer.Enums.SensorType.Temperature)
                .Select(r => r.Value)
                .ToList();

            var occupancies = latestReadings
                .Where(r => r.Sensor?.Type == EntityLayer.Enums.SensorType.Occupancy)
                .Select(r => r.Value)
                .ToList();

            var dashboard = new SensorDashboardDto
            {
                TotalSensors = sensors.Count,
                OnlineSensors = sensors.Count(s => s.IsOnline),
                OfflineSensors = sensors.Count(s => !s.IsOnline),
                LatestReadings = latestReadings.Select(r => new SensorReadingDto
                {
                    SensorId = r.Sensor?.SensorId ?? "",
                    SensorName = r.Sensor?.Name ?? "",
                    Type = r.Sensor?.Type.ToString() ?? "",
                    Value = r.Value,
                    Unit = r.Unit,
                    Timestamp = r.Timestamp
                }).ToList(),
                Environment = new EnvironmentSummaryDto
                {
                    AverageTemperature = temperatures.Any() ? Math.Round(temperatures.Average(), 1) : 0,
                    MinTemperature = temperatures.Any() ? Math.Round(temperatures.Min(), 1) : 0,
                    MaxTemperature = temperatures.Any() ? Math.Round(temperatures.Max(), 1) : 0,
                    AverageOccupancy = occupancies.Any() ? Math.Round(occupancies.Average(), 1) : 0,
                    TotalClassrooms = occupancies.Count,
                    OccupiedClassrooms = occupancies.Count(o => o > 50)
                }
            };

            return Ok(dashboard);
        }

        /// <summary>
        /// Belirli sensörün son okumalarını getirir
        /// </summary>
        [HttpGet("{sensorId}/readings")]
        public async Task<IActionResult> GetSensorReadings(string sensorId, [FromQuery] int limit = 50)
        {
            var sensor = await _context.Sensors
                .FirstOrDefaultAsync(s => s.SensorId == sensorId && s.IsActive);

            if (sensor == null)
                return NotFound("Sensör bulunamadı");

            var readings = await _context.SensorReadings
                .Where(r => r.SensorId == sensor.Id && r.IsActive)
                .OrderByDescending(r => r.Timestamp)
                .Take(limit)
                .Select(r => new SensorReadingDto
                {
                    SensorId = sensorId,
                    SensorName = sensor.Name,
                    Type = sensor.Type.ToString(),
                    Value = r.Value,
                    Unit = r.Unit,
                    Timestamp = r.Timestamp
                })
                .ToListAsync();

            return Ok(readings);
        }
    }
}
