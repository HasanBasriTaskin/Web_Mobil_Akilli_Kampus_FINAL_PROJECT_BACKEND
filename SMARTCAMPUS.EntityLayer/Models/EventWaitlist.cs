using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class EventWaitlist : BaseEntity
    {
        public int EventId { get; set; }
        
        [ForeignKey("EventId")]
        public Event Event { get; set; } = null!;
        
        [Required]
        public string UserId { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        
        public int QueuePosition { get; set; }
        
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsNotified { get; set; } = false;
    }
}
