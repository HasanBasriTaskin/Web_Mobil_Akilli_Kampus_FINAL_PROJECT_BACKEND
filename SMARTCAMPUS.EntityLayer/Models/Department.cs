using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Department : BaseEntity
    {
        // Id inherited from BaseEntity
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? FacultyName { get; set; } // e.g. "Engineering Faculty"
        public string? Description { get; set; }
        
        // Navigation Properties
        public ICollection<Student>? Students { get; set; }
        public ICollection<Faculty>? FacultyMembers { get; set; }
    }
}
