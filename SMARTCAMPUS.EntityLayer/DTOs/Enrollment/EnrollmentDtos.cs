using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Enrollment
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public EnrollmentStatus Status { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public double? MidtermGrade { get; set; }
        public double? FinalGrade { get; set; }
        public string? LetterGrade { get; set; }
        public double? GradePoint { get; set; }
        
        // Student Info
        public int StudentId { get; set; }
        public string StudentNumber { get; set; } = null!;
        public string StudentName { get; set; } = null!;
        
        // Section Info
        public int SectionId { get; set; }
        public string SectionNumber { get; set; } = null!;
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string InstructorName { get; set; } = null!;
    }

    public class CreateEnrollmentDto
    {
        public int SectionId { get; set; }
    }

    public class StudentCourseDto
    {
        public int EnrollmentId { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string SectionNumber { get; set; } = null!;
        public string InstructorName { get; set; } = null!;
        public int Credits { get; set; }
        public string? ScheduleJson { get; set; }
        public EnrollmentStatus Status { get; set; }
        public double? MidtermGrade { get; set; }
        public double? FinalGrade { get; set; }
        public string? LetterGrade { get; set; }
    }

    public class SectionStudentDto
    {
        public int StudentId { get; set; }
        public int EnrollmentId { get; set; }
        public string StudentNumber { get; set; } = null!;
        public string StudentName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime EnrollmentDate { get; set; }
        public EnrollmentStatus Status { get; set; }
        public double? MidtermGrade { get; set; }
        public double? FinalGrade { get; set; }
        public string? LetterGrade { get; set; }
    }

    public class PendingEnrollmentDto
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string StudentNumber { get; set; } = null!;
        public string StudentName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime RequestDate { get; set; }
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string SectionNumber { get; set; } = null!;
    }

    public class RejectEnrollmentDto
    {
        public string? Reason { get; set; }
    }

    public class FacultySectionDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string SectionNumber { get; set; } = null!;
        public string Semester { get; set; } = null!;
        public int Year { get; set; }
        public int Capacity { get; set; }
        public int EnrolledCount { get; set; }
        public int PendingCount { get; set; }
    }
}
