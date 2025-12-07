using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        
        // Navigation Properties
        public ICollection<Student>? Students { get; set; }
        public ICollection<Faculty>? FacultyMembers { get; set; }
    }
}
