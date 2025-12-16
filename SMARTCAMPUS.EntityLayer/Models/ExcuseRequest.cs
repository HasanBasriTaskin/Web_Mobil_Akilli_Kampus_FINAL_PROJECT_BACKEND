using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class ExcuseRequest : BaseEntity
    {
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = null!;
        
        [MaxLength(500)]
        public string? DocumentUrl { get; set; }
        
        public ExcuseRequestStatus Status { get; set; } = ExcuseRequestStatus.Pending;
        
        public DateTime? ReviewedAt { get; set; }
        
        [MaxLength(1000)]
        public string? Notes { get; set; }
        
        // Foreign Keys
        public int StudentId { get; set; }
        
        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;
        
        public int SessionId { get; set; }
        
        [ForeignKey("SessionId")]
        public AttendanceSession Session { get; set; } = null!;
        
        public string? ReviewedById { get; set; }
        
        [ForeignKey("ReviewedById")]
        public User? ReviewedBy { get; set; }
    }
}
