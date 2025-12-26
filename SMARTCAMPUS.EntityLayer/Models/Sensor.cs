using System.ComponentModel.DataAnnotations;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.Models
{
    /// <summary>
    /// Kampüs sensörlerini temsil eden entity
    /// </summary>
    public class Sensor : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string SensorId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public SensorType Type { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public int? ClassroomId { get; set; }
        public virtual Classroom? Classroom { get; set; }

        public bool IsOnline { get; set; } = true;
        public DateTime? LastReading { get; set; }
    }

    /// <summary>
    /// Sensör okuma değerlerini saklayan entity
    /// </summary>
    public class SensorReading : BaseEntity
    {
        public int SensorId { get; set; }
        public virtual Sensor? Sensor { get; set; }

        public double Value { get; set; }
        public string? Unit { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
