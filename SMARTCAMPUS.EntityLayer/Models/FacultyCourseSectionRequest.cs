using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    /// <summary>
    /// Akademisyenlerin ders alma taleplerini tutar
    /// Faculty -> Section atama isteği, Admin onayı gerektirir
    /// </summary>
    public class FacultyCourseSectionRequest : BaseEntity
    {

        [Required]
        public int FacultyId { get; set; }

        [ForeignKey("FacultyId")]
        public virtual Faculty Faculty { get; set; } = null!;

        [Required]
        public int SectionId { get; set; }

        [ForeignKey("SectionId")]
        public virtual CourseSection Section { get; set; } = null!;

        /// <summary>
        /// Pending, Approved, Rejected
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        public DateTime? ResponseDate { get; set; }

        /// <summary>
        /// Admin notu (onay/red sebebi)
        /// </summary>
        [MaxLength(500)]
        public string? AdminNote { get; set; }

        /// <summary>
        /// İşlemi yapan admin ID
        /// </summary>
        [MaxLength(450)]
        public string? ProcessedByAdminId { get; set; }
    }
}
