namespace SMARTCAMPUS.EntityLayer.DTOs.Grade
{
    public class GradeEntryDto
    {
        public int EnrollmentId { get; set; }
        public double? MidtermGrade { get; set; }
        public double? FinalGrade { get; set; }
    }

    public class StudentGradeDto
    {
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public int Credits { get; set; }
        public string Semester { get; set; } = null!;
        public int Year { get; set; }
        public double? MidtermGrade { get; set; }
        public double? FinalGrade { get; set; }
        public string? LetterGrade { get; set; }
        public double? GradePoint { get; set; }
    }

    public class TranscriptDto
    {
        public string StudentNumber { get; set; } = null!;
        public string StudentName { get; set; } = null!;
        public string DepartmentName { get; set; } = null!;
        public double CGPA { get; set; }
        public int TotalCredits { get; set; }
        public int TotalECTS { get; set; }
        public List<SemesterGradesDto> Semesters { get; set; } = new();
    }

    public class SemesterGradesDto
    {
        public string Semester { get; set; } = null!;
        public int Year { get; set; }
        public double GPA { get; set; }
        public int Credits { get; set; }
        public List<StudentGradeDto> Courses { get; set; } = new();
    }
}
