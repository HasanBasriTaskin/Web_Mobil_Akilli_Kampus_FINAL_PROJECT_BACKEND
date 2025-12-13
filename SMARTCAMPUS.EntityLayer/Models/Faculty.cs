using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Faculty : BaseEntity
    {
        // Id inherited from BaseEntity
        public string EmployeeNumber { get; set; } = null!;
        public string Title { get; set; } = null!; // Dr., Prof., etc.
        public string? OfficeLocation { get; set; }
        
        // Foreign Keys
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        
        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; } = null!;
    }
}
