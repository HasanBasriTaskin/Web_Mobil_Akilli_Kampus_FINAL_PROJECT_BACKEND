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
        
        public bool IsFlagged { get; set; } = false;
        public string? FlagReason { get; set; } // "Outside geofence", "Late check-in", etc.
    }
}

