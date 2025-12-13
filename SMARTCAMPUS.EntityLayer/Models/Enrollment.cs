using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Enrollment : BaseEntity
    {
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;
        
        public int SectionId { get; set; }
        [ForeignKey("SectionId")]
        public CourseSection Section { get; set; } = null!;
        
        public string Status { get; set; } = "Active"; // "Active", "Dropped", "Completed", "Failed"
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        
        // Grades
        public decimal? MidtermGrade { get; set; }
        public decimal? FinalGrade { get; set; }
        public string? LetterGrade { get; set; } // "A", "B", "C", "D", "F", etc.
        public decimal? GradePoint { get; set; } // 4.0 scale
    }
}

