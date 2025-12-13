using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class CourseSection : BaseEntity
    {
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; } = null!;
        
        public string SectionNumber { get; set; } = null!; // e.g., "A", "B", "01"
        public string Semester { get; set; } = null!; // "Fall", "Spring", "Summer"
        public int Year { get; set; }
        
        public string? InstructorId { get; set; } // UserId of Faculty
        [ForeignKey("InstructorId")]
        public User? Instructor { get; set; }
        
        public int Capacity { get; set; }
        public int EnrolledCount { get; set; } = 0;
        
        public string? ScheduleJson { get; set; } // JSON format: [{"day": "Monday", "startTime": "09:00", "endTime": "10:30", "classroomId": 1}]
        
        public int? ClassroomId { get; set; }
        [ForeignKey("ClassroomId")]
        public Classroom? Classroom { get; set; }
        
        // Navigation Properties
        public ICollection<Enrollment>? Enrollments { get; set; }
        public ICollection<AttendanceSession>? AttendanceSessions { get; set; }
    }
}

