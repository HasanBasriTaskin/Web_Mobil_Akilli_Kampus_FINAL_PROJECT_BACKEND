using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Enrollment : BaseEntity
    {
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Enrolled;
        
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        
        [Range(0, 100)]
        public double? MidtermGrade { get; set; }
        
        [Range(0, 100)]
        public double? FinalGrade { get; set; }
        
        [MaxLength(5)]
        public string? LetterGrade { get; set; }
        
        [Range(0, 4)]
        public double? GradePoint { get; set; }
        
        // Foreign Keys
        public int StudentId { get; set; }
        
        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;
        
        public int SectionId { get; set; }
        
        [ForeignKey("SectionId")]
        public CourseSection Section { get; set; } = null!;
    }
}
