using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class ClassroomReservation : BaseEntity
    {
        public int ClassroomId { get; set; }
        
        [ForeignKey("ClassroomId")]
        public Classroom Classroom { get; set; } = null!;
        
        [Required]
        public string RequestedByUserId { get; set; } = null!;
        
        [ForeignKey("RequestedByUserId")]
        public User RequestedBy { get; set; } = null!;
        
        [MaxLength(100)]
        public string? StudentLeaderName { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Purpose { get; set; } = null!;
        
        [Required]
        public DateTime ReservationDate { get; set; }
        
        public TimeSpan StartTime { get; set; }
        
        public TimeSpan EndTime { get; set; }
        
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    }
}
