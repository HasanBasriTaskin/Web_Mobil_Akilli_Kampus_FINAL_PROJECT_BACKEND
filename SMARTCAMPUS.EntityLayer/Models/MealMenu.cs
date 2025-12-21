using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class MealMenu : BaseEntity
    {
        public int CafeteriaId { get; set; }
        
        [ForeignKey("CafeteriaId")]
        public Cafeteria Cafeteria { get; set; } = null!;
        
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public MealType MealType { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        
        public bool IsPublished { get; set; } = false;
        
        // Navigation Properties
        public ICollection<MealMenuItem> MenuItems { get; set; } = new List<MealMenuItem>();
        public MealNutrition? Nutrition { get; set; }
        public ICollection<MealReservation> Reservations { get; set; } = new List<MealReservation>();
    }
}
