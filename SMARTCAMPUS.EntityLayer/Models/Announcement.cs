using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Announcement : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? TargetAudience { get; set; } // "All", "Students", "Faculty", "Department"
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }
        
        public string? CreatedById { get; set; } // UserId of creator
        [ForeignKey("CreatedById")]
        public User? CreatedBy { get; set; }
        
        public DateTime PublishDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; }
        public bool IsImportant { get; set; }
        public int ViewCount { get; set; } = 0;
    }
}

