using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class AttendanceSession : BaseEntity
    {
        public DateTime Date { get; set; }
        
        public TimeSpan StartTime { get; set; }
        
        public TimeSpan EndTime { get; set; }
        
        [Range(-90, 90)]
        public double Latitude { get; set; }
        
        [Range(-180, 180)]
        public double Longitude { get; set; }
        
        [Range(5, 100)]
        public int GeofenceRadius { get; set; } = 15;
        
        [MaxLength(500)]
        public string? QRCode { get; set; }
        
        public AttendanceSessionStatus Status { get; set; } = AttendanceSessionStatus.Open;
        
        // Foreign Keys
        public int SectionId { get; set; }
        
        [ForeignKey("SectionId")]
        public CourseSection Section { get; set; } = null!;
        
        public int InstructorId { get; set; }
        
        [ForeignKey("InstructorId")]
        public Faculty Instructor { get; set; } = null!;
        
        // Navigation Properties
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public ICollection<ExcuseRequest> ExcuseRequests { get; set; } = new List<ExcuseRequest>();
    }
}
