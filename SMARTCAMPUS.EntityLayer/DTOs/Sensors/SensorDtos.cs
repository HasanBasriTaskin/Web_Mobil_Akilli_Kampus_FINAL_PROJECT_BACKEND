namespace SMARTCAMPUS.EntityLayer.DTOs.Sensors
{
    public class SensorDto
    {
        public int Id { get; set; }
        public string SensorId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Location { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastReading { get; set; }
    }

    public class SensorReadingDto
    {
        public string SensorId { get; set; } = string.Empty;
        public string SensorName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double Value { get; set; }
        public string? Unit { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class SensorDashboardDto
    {
        public int TotalSensors { get; set; }
        public int OnlineSensors { get; set; }
        public int OfflineSensors { get; set; }
        public List<SensorReadingDto> LatestReadings { get; set; } = new();
        public EnvironmentSummaryDto Environment { get; set; } = new();
    }

    public class EnvironmentSummaryDto
    {
        public double AverageTemperature { get; set; }
        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }
        public double AverageOccupancy { get; set; }
        public int TotalClassrooms { get; set; }
        public int OccupiedClassrooms { get; set; }
    }
}
