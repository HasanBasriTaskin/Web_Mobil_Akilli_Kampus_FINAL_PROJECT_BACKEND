using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class CoursePrerequisite
    {
        public int CourseId { get; set; }
        
        [ForeignKey("CourseId")]
        public Course Course { get; set; } = null!;
        
        public int PrerequisiteCourseId { get; set; }
        
        [ForeignKey("PrerequisiteCourseId")]
        public Course PrerequisiteCourse { get; set; } = null!;
    }
}
