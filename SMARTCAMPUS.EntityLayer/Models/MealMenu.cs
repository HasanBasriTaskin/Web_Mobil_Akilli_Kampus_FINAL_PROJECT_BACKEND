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
        
        /// <summary>
        /// JSON array of menu items: ["Mercimek Çorbası", "Pilav", "Tavuk Sote"]
        /// </summary>
        [Required]
        public string ItemsJson { get; set; } = null!;
        
        /// <summary>
        /// JSON object with nutrition info: {"calories": 850, "protein": 25, ...}
        /// </summary>
        public string? NutritionJson { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        
        public bool IsPublished { get; set; } = false;
        
        // Navigation Properties
        public ICollection<MealReservation> Reservations { get; set; } = new List<MealReservation>();
    }
}
