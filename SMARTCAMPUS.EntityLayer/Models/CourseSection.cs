using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class CourseSection : BaseEntity
    {
        [Required]
        [MaxLength(10)]
        public string SectionNumber { get; set; } = null!;
        
        [Required]
        [MaxLength(20)]
        public string Semester { get; set; } = null!;
        
        [Range(2020, 2100)]
        public int Year { get; set; }
        
        [Range(1, 500)]
        public int Capacity { get; set; }
        
        public int EnrolledCount { get; set; } = 0;
        
        public string? ScheduleJson { get; set; }
        
        // Foreign Keys
        public int CourseId { get; set; }
        
        [ForeignKey("CourseId")]
        public Course Course { get; set; } = null!;
        
        public int InstructorId { get; set; }
        
        [ForeignKey("InstructorId")]
        public Faculty Instructor { get; set; } = null!;
        
        // Navigation Properties
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<AttendanceSession> AttendanceSessions { get; set; } = new List<AttendanceSession>();
    }
}
