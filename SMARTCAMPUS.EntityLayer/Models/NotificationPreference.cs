using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.Models
{
    /// <summary>
    /// Kullanıcı bildirim tercihleri
    /// </summary>
    public class NotificationPreference : BaseEntity
    {
        public NotificationCategory Category { get; set; }

        /// <summary>
        /// Uygulama içi bildirim açık mı
        /// </summary>
        public bool InAppEnabled { get; set; } = true;

        /// <summary>
        /// E-posta bildirimi açık mı
        /// </summary>
        public bool EmailEnabled { get; set; } = true;

        // Foreign Keys
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
