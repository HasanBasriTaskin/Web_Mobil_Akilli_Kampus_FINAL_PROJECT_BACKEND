using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Schedule : BaseEntity
    {
        public int SectionId { get; set; }
        
        [ForeignKey("SectionId")]
        public CourseSection Section { get; set; } = null!;
        
        public int ClassroomId { get; set; }
        
        [ForeignKey("ClassroomId")]
        public Classroom Classroom { get; set; } = null!;
        
        public DayOfWeek DayOfWeek { get; set; }
        
        public TimeSpan StartTime { get; set; }
        
        public TimeSpan EndTime { get; set; }
    }
}
