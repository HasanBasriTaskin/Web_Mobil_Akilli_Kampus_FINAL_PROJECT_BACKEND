using System.ComponentModel.DataAnnotations;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Cafeteria : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        
        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = null!;
        
        [Range(1, 2000)]
        public int Capacity { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public ICollection<MealMenu> Menus { get; set; } = new List<MealMenu>();
        public ICollection<MealReservation> Reservations { get; set; } = new List<MealReservation>();
    }
}
