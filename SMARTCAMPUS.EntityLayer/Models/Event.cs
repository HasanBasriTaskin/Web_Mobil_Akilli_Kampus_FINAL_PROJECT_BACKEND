using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Event : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;
        
        [Required]
        public string Description { get; set; } = null!;
        
        public int CategoryId { get; set; }
        
        [ForeignKey("CategoryId")]
        public EventCategory Category { get; set; } = null!;
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = null!;
        
        [Range(1, 10000)]
        public int Capacity { get; set; }
        
        public int RegisteredCount { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; } = 0; // 0 = Ücretsiz
        
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        
        [Required]
        public string CreatedByUserId { get; set; } = null!;
        
        [ForeignKey("CreatedByUserId")]
        public User CreatedBy { get; set; } = null!;
        
        // Optimistic Locking için RowVersion
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
        
        // Navigation Properties
        public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
        public ICollection<EventWaitlist> Waitlists { get; set; } = new List<EventWaitlist>();
    }
}
