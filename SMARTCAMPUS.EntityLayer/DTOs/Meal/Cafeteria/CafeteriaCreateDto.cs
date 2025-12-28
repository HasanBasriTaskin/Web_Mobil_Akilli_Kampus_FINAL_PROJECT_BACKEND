namespace SMARTCAMPUS.EntityLayer.DTOs.Meal.Cafeteria
{
    public class CafeteriaCreateDto
    {
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public int Capacity { get; set; }
    }
}
