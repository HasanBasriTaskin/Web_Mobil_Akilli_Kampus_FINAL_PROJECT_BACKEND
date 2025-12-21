using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Attendance
{
    public class ExcuseRequestDto
    {
        public int Id { get; set; }
        public string Reason { get; set; } = null!;
        public string? DocumentUrl { get; set; }
        public ExcuseRequestStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? Notes { get; set; }
        
        // Student Info
        public int StudentId { get; set; }
        public string StudentNumber { get; set; } = null!;
        public string StudentName { get; set; } = null!;
        
        // Session Info
        public int SessionId { get; set; }
        public DateTime SessionDate { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        
        // Reviewer Info
        public string? ReviewedByName { get; set; }
    }

    public class CreateExcuseRequestDto
    {
        public int SessionId { get; set; }
        public string Reason { get; set; } = null!;
    }

    public class ReviewExcuseRequestDto
    {
        public string? Notes { get; set; }
    }
}
