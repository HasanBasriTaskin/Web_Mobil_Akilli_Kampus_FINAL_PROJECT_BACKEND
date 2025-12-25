using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Student : BaseEntity
    {
        // Id inherited from BaseEntity
        public string StudentNumber { get; set; } = null!;
        public double GPA { get; set; }
        public double CGPA { get; set; }
        
        // Part 3 - Scholarship & Meal Quota
        public bool HasScholarship { get; set; } = false;
        public int DailyMealQuota { get; set; } = 2;
        
        // Foreign Keys
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        
        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; } = null!;
        
        // Navigation Properties
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public ICollection<ExcuseRequest> ExcuseRequests { get; set; } = new List<ExcuseRequest>();
    }
}
