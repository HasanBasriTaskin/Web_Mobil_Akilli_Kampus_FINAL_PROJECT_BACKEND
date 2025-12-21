using System.ComponentModel.DataAnnotations;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class EventCategory : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(50)]
        public string? IconName { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
