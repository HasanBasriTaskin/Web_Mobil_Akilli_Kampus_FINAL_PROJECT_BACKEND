namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class GradeDto
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string? StudentNumber { get; set; }
        public string? StudentName { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public decimal? MidtermGrade { get; set; }
        public decimal? FinalGrade { get; set; }
        public string? LetterGrade { get; set; }
        public decimal? GradePoint { get; set; }
    }

    public class GradeUpdateDto
    {
        public int EnrollmentId { get; set; }
        public decimal? MidtermGrade { get; set; }
        public decimal? FinalGrade { get; set; }
    }

    public class GradeBulkUpdateDto
    {
        public List<GradeUpdateDto> Grades { get; set; } = new();
    }
}

