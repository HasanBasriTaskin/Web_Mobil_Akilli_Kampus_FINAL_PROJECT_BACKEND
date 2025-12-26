using System.ComponentModel.DataAnnotations;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.Models
{
    /// <summary>
    /// Admin tarafından oluşturulan yemek içerikleri (master data)
    /// Menü oluşturulurken bu içeriklerden seçilir
    /// </summary>
    public class FoodItem : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public MealItemCategory Category { get; set; }
        
        /// <summary>
        /// Kalori bilgisi (opsiyonel)
        /// </summary>
        public int? Calories { get; set; }

        // Navigation Properties
        public ICollection<MealMenuItem> MenuItems { get; set; } = new List<MealMenuItem>();
    }
}
