using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Meal.FoodItem
{
    public class FoodItemCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public MealItemCategory Category { get; set; }
        public int? Calories { get; set; }
    }
}
