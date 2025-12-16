using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Course : BaseEntity
    {
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = null!;
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;
        
        [MaxLength(2000)]
        public string? Description { get; set; }
        
        [Range(1, 10)]
        public int Credits { get; set; }
        
        [Range(1, 30)]
        public int ECTS { get; set; }
        
        [MaxLength(500)]
        public string? SyllabusUrl { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Foreign Keys
        public int DepartmentId { get; set; }
        
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; } = null!;
        
        // Navigation Properties
        public ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
        public ICollection<CoursePrerequisite> Prerequisites { get; set; } = new List<CoursePrerequisite>();
        public ICollection<CoursePrerequisite> RequiredFor { get; set; } = new List<CoursePrerequisite>();
    }
}
