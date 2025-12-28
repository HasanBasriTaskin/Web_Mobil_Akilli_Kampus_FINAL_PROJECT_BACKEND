using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    /// <summary>
    /// Menü ile FoodItem arasındaki ilişki tablosu
    /// Bir menüde hangi yemeklerin olduğunu tutar
    /// </summary>
    public class MealMenuItem : BaseEntity
    {
        public int MenuId { get; set; }
        
        [ForeignKey("MenuId")]
        public MealMenu Menu { get; set; } = null!;
        
        public int FoodItemId { get; set; }
        
        [ForeignKey("FoodItemId")]
        public FoodItem FoodItem { get; set; } = null!;
        
        /// <summary>
        /// Menü içindeki sıralama (çorba önce, tatlı sonda gibi)
        /// </summary>
        public int OrderIndex { get; set; } = 0;
    }
}
