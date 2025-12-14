namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class TranscriptDto
    {
        public int StudentId { get; set; }
        public string? StudentNumber { get; set; }
        public string? StudentName { get; set; }
        public string? DepartmentName { get; set; }
        public double GPA { get; set; }
        public double CGPA { get; set; }
        public List<TranscriptCourseDto> Courses { get; set; } = new();
        public int TotalCredits { get; set; }
        public int TotalECTS { get; set; }
    }

    public class TranscriptCourseDto
    {
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public int Credits { get; set; }
        public int ECTS { get; set; }
        public string Semester { get; set; } = null!;
        public int Year { get; set; }
        public string? LetterGrade { get; set; }
        public decimal? GradePoint { get; set; }
    }
}



