namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class ExcuseRequestDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string? StudentNumber { get; set; }
        public string? StudentName { get; set; }
        public int SessionId { get; set; }
        public DateTime? SessionDate { get; set; }
        public string? CourseCode { get; set; }
        public string Reason { get; set; } = null!;
        public string? DocumentUrl { get; set; }
        public string Status { get; set; } = null!;
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class ExcuseRequestCreateDto
    {
        public int SessionId { get; set; }
        public string Reason { get; set; } = null!;
        public string? DocumentUrl { get; set; }
    }

    public class ExcuseRequestReviewDto
    {
        public int RequestId { get; set; }
        public string Status { get; set; } = null!; // "Approved" or "Rejected"
        public string? Notes { get; set; }
    }
}

