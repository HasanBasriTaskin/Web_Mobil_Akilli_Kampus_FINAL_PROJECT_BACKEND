namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class EnrollmentRequestDto
    {
        public int SectionId { get; set; }
    }

    public class EnrollmentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public EnrollmentDto? Enrollment { get; set; }
        public List<string>? Conflicts { get; set; } // Schedule conflicts
        public List<string>? MissingPrerequisites { get; set; }
    }
}



