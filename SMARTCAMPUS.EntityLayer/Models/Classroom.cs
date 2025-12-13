namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Classroom : BaseEntity
    {
        public string Building { get; set; } = null!; // e.g., "A", "B", "Engineering"
        public string RoomNumber { get; set; } = null!; // e.g., "101", "A-205"
        public int Capacity { get; set; }
        public string? FeaturesJson { get; set; } // JSON format: {"projector": true, "computer": true, "whiteboard": true}
        
        // Navigation Properties
        public ICollection<CourseSection>? Sections { get; set; }
    }
}



