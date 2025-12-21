using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class MealNutrition : BaseEntity
    {
        public int MenuId { get; set; }
        
        [ForeignKey("MenuId")]
        public MealMenu Menu { get; set; } = null!;
        
        public int Calories { get; set; }
        
        public int Protein { get; set; }
        
        public int Carbohydrates { get; set; }
        
        public int Fat { get; set; }
        
        public int? Fiber { get; set; }
        
        public int? Sodium { get; set; }
    }
}
