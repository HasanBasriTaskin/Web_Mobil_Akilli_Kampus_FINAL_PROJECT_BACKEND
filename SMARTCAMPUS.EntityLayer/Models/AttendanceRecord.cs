using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class AttendanceRecord : BaseEntity
    {
        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
        
        [Range(-90, 90)]
        public double Latitude { get; set; }
        
        [Range(-180, 180)]
        public double Longitude { get; set; }
        
        [Range(0, 10000)]
        public double DistanceFromCenter { get; set; }
        
        public bool IsFlagged { get; set; } = false;
        
        [MaxLength(500)]
        public string? FlagReason { get; set; }
        
        // Foreign Keys
        public int SessionId { get; set; }
        
        [ForeignKey("SessionId")]
        public AttendanceSession Session { get; set; } = null!;
        
        public int StudentId { get; set; }
        
        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;
    }
}
