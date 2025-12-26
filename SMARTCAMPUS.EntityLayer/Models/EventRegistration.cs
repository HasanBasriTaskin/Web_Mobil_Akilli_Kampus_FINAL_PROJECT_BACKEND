using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class EventRegistration : BaseEntity
    {
        public int EventId { get; set; }
        
        [ForeignKey("EventId")]
        public Event Event { get; set; } = null!;
        
        [Required]
        public string UserId { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [MaxLength(50)]
        public string QRCode { get; set; } = null!;
        
        public bool CheckedIn { get; set; } = false;
        
        public DateTime? CheckedInAt { get; set; }
    }
}
