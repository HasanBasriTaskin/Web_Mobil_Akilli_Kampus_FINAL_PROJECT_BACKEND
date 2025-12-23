using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Meal.FoodItem
{
    public class FoodItemUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public MealItemCategory? Category { get; set; }
        public int? Calories { get; set; }
        public bool? IsActive { get; set; }
    }
}
