using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Meal.Menu
{
    public class MealMenuItemDto
    {
        public int Id { get; set; }
        public int FoodItemId { get; set; }
        public string FoodItemName { get; set; } = null!;
        public MealItemCategory Category { get; set; }
        public string CategoryName => Category.ToString();
        public int? Calories { get; set; }
        public int OrderIndex { get; set; }
    }
}
