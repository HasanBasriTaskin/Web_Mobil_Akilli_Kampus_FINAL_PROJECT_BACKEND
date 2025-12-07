using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Student : BaseEntity
    {
        // Id inherited from BaseEntity
        public string StudentNumber { get; set; } = null!;
        public double GPA { get; set; }
        public double CGPA { get; set; }
        
        // Foreign Keys
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        
        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; } = null!;
    }
}
