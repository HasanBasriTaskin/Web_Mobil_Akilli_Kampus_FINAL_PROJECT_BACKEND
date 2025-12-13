using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class ExcuseRequest : BaseEntity
    {
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;
        
        public int SessionId { get; set; }
        [ForeignKey("SessionId")]
        public AttendanceSession Session { get; set; } = null!;
        
        public string Reason { get; set; } = null!;
        public string? DocumentUrl { get; set; } // URL to uploaded document
        
        public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected"
        
        public string? ReviewedBy { get; set; } // UserId of reviewer
        [ForeignKey("ReviewedBy")]
        public User? Reviewer { get; set; }
        
        public DateTime? ReviewedAt { get; set; }
        public string? Notes { get; set; } // Reviewer notes
    }
}



