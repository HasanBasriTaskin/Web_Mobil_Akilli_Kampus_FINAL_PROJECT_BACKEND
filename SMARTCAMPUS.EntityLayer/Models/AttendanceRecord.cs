using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class AttendanceRecord : BaseEntity
    {
        public int SessionId { get; set; }
        [ForeignKey("SessionId")]
        public AttendanceSession Session { get; set; } = null!;
        
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;
        
        public DateTime? CheckInTime { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? DistanceFromCenter { get; set; } // in meters
        
        // GPS Spoofing Detection
        public string? IpAddress { get; set; }
        public bool IsMockLocation { get; set; } = false;
        public decimal? Velocity { get; set; } // km/h - calculated from previous location
        public string? DeviceInfo { get; set; } // JSON: {"sensors": {...}, "browser": "..."}
        public int FraudScore { get; set; } = 0; // 0-100, higher = more suspicious
        
        public bool IsFlagged { get; set; } = false;
        public string? FlagReason { get; set; } // "Outside geofence", "Late check-in", "GPS spoofing", etc.
    }
}



