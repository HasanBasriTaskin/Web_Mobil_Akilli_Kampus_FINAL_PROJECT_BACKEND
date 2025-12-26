using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.API.Hubs;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs.Sensors;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.API.BackgroundServices
{
    /// <summary>
    /// IoT sensörlerinden veri simüle eden ve SignalR ile yayınlayan background service
    /// </summary>
    public class SensorSimulationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SensorSimulationService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);
        private readonly Random _random = new();

        public SensorSimulationService(
            IServiceProvider serviceProvider,
            IHubContext<NotificationHub> hubContext,
            ILogger<SensorSimulationService> logger)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SensorSimulationService started.");

            // İlk başlatmada sensörlerin var olduğundan emin ol
            await EnsureSensorsExistAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var readings = await GenerateSensorReadingsAsync();
                    
                    // SignalR ile tüm kullanıcılara yayınla
                    await _hubContext.Clients.All.SendAsync("SensorUpdate", readings, stoppingToken);
                    
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in SensorSimulationService");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }

            _logger.LogInformation("SensorSimulationService stopped.");
        }

        private async Task EnsureSensorsExistAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CampusContext>();

            if (await context.Sensors.AnyAsync())
                return;

            // Simüle edilmiş sensörler oluştur
            var sensors = new List<Sensor>
            {
                new() { SensorId = "TEMP-01", Name = "Merkez Bina Sıcaklık", Type = SensorType.Temperature, Location = "A Blok Giriş", IsOnline = true },
                new() { SensorId = "TEMP-02", Name = "Kütüphane Sıcaklık", Type = SensorType.Temperature, Location = "Kütüphane 1. Kat", IsOnline = true },
                new() { SensorId = "TEMP-03", Name = "Yemekhane Sıcaklık", Type = SensorType.Temperature, Location = "Yemekhane", IsOnline = true },
                new() { SensorId = "OCC-01", Name = "Lab-101 Doluluk", Type = SensorType.Occupancy, Location = "B Blok Lab 101", IsOnline = true },
                new() { SensorId = "OCC-02", Name = "Lab-102 Doluluk", Type = SensorType.Occupancy, Location = "B Blok Lab 102", IsOnline = true },
                new() { SensorId = "OCC-03", Name = "Konferans Salonu Doluluk", Type = SensorType.Occupancy, Location = "A Blok Konferans", IsOnline = true },
                new() { SensorId = "HUM-01", Name = "Sera Nem", Type = SensorType.Humidity, Location = "Sera", IsOnline = true },
                new() { SensorId = "LIGHT-01", Name = "Koridor Işık", Type = SensorType.Light, Location = "B Blok Koridor", IsOnline = true },
            };

            foreach (var sensor in sensors)
            {
                sensor.CreatedDate = DateTime.UtcNow;
                sensor.IsActive = true;
            }

            context.Sensors.AddRange(sensors);
            await context.SaveChangesAsync();
            _logger.LogInformation("Created {Count} simulated sensors", sensors.Count);
        }

        private async Task<SensorDashboardDto> GenerateSensorReadingsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CampusContext>();

            var sensors = await context.Sensors.Where(s => s.IsActive && s.IsOnline).ToListAsync();
            var readings = new List<SensorReadingDto>();
            var temperatures = new List<double>();
            var occupancies = new List<double>();

            foreach (var sensor in sensors)
            {
                double value;
                string unit;

                switch (sensor.Type)
                {
                    case SensorType.Temperature:
                        value = 18 + _random.NextDouble() * 10; // 18-28°C
                        unit = "°C";
                        temperatures.Add(value);
                        break;
                    case SensorType.Humidity:
                        value = 40 + _random.NextDouble() * 30; // 40-70%
                        unit = "%";
                        break;
                    case SensorType.Occupancy:
                        value = _random.Next(0, 101); // 0-100%
                        unit = "%";
                        occupancies.Add(value);
                        break;
                    case SensorType.Light:
                        value = 200 + _random.NextDouble() * 600; // 200-800 lux
                        unit = "lux";
                        break;
                    default:
                        value = _random.NextDouble() * 100;
                        unit = "";
                        break;
                }

                // Veritabanına kaydet
                var reading = new SensorReading
                {
                    SensorId = sensor.Id,
                    Value = Math.Round(value, 2),
                    Unit = unit,
                    Timestamp = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };
                context.SensorReadings.Add(reading);

                // DTO'ya ekle
                readings.Add(new SensorReadingDto
                {
                    SensorId = sensor.SensorId,
                    SensorName = sensor.Name,
                    Type = sensor.Type.ToString(),
                    Value = Math.Round(value, 2),
                    Unit = unit,
                    Timestamp = DateTime.UtcNow
                });

                sensor.LastReading = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();

            return new SensorDashboardDto
            {
                TotalSensors = sensors.Count,
                OnlineSensors = sensors.Count(s => s.IsOnline),
                OfflineSensors = sensors.Count(s => !s.IsOnline),
                LatestReadings = readings,
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
        }
    }
}
