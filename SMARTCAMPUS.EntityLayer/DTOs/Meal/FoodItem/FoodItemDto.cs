using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Meal.FoodItem
{
    public class FoodItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public MealItemCategory Category { get; set; }
        public string CategoryName => Category.ToString();
        public int? Calories { get; set; }
        public bool IsActive { get; set; }
    }
}
