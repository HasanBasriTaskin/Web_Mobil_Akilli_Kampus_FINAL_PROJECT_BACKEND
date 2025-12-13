using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class CoursePrerequisite : BaseEntity
    {
        public int CourseId { get; set; } // Course that requires the prerequisite
        [ForeignKey("CourseId")]
        public Course Course { get; set; } = null!;
        
        public int PrerequisiteCourseId { get; set; } // Required course
        [ForeignKey("PrerequisiteCourseId")]
        public Course PrerequisiteCourse { get; set; } = null!;
    }
}



