namespace SMARTCAMPUS.EntityLayer.DTOs.Event
{
    public class EventCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconName { get; set; }
        public bool IsActive { get; set; }
    }

    public class EventCategoryCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconName { get; set; }
    }

    public class EventCategoryUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? IconName { get; set; }
    }
}

