namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string? StudentNumber { get; set; }
        public string? StudentName { get; set; }
        public int SectionId { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? SectionNumber { get; set; }
        public string Status { get; set; } = null!;
        public DateTime EnrollmentDate { get; set; }
        public decimal? MidtermGrade { get; set; }
        public decimal? FinalGrade { get; set; }
        public string? LetterGrade { get; set; }
        public decimal? GradePoint { get; set; }
    }

    public class EnrollmentCreateDto
    {
        public int SectionId { get; set; }
    }

    public class EnrollmentDropDto
    {
        public int EnrollmentId { get; set; }
    }
}

