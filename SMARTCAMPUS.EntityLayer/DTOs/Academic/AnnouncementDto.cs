namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class AnnouncementDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? TargetAudience { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsImportant { get; set; }
        public int ViewCount { get; set; }
    }
}

