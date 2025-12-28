namespace SMARTCAMPUS.EntityLayer.DTOs.Meal.Cafeteria
{
    public class CafeteriaDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
    }
}
