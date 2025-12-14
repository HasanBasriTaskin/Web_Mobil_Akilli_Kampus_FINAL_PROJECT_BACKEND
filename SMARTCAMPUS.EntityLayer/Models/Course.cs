using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Course : BaseEntity
    {
        public string Code { get; set; } = null!; // e.g., "CS101"
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int Credits { get; set; }
        public int ECTS { get; set; }
        public string? SyllabusUrl { get; set; }
        
        // Foreign Keys
        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; } = null!;
        
        // Navigation Properties
        public ICollection<CourseSection>? Sections { get; set; }
        public ICollection<CoursePrerequisite>? Prerequisites { get; set; } // Courses that require this course
        public ICollection<CoursePrerequisite>? RequiredBy { get; set; } // Courses that this course requires
    }
}



