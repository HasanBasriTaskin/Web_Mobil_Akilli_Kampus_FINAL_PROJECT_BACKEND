using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class AttendanceSession : BaseEntity
    {
        public int SectionId { get; set; }
        [ForeignKey("SectionId")]
        public CourseSection Section { get; set; } = null!;
        
        public string? InstructorId { get; set; } // UserId of Faculty
        [ForeignKey("InstructorId")]
        public User? Instructor { get; set; }
        
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        
        // Geofencing
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? GeofenceRadius { get; set; } // in meters
        
        public string? QrCode { get; set; }
        public string Status { get; set; } = "Scheduled"; // "Scheduled", "Active", "Completed", "Cancelled"
        
        // Navigation Properties
        public ICollection<AttendanceRecord>? AttendanceRecords { get; set; }
        public ICollection<ExcuseRequest>? ExcuseRequests { get; set; }
    }
}



