using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class PasswordResetToken : BaseEntity
    {
        // Id inherited
        public string Token { get; set; } = null!; // Hashed or original token
        // CreatedDate inherited
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        
        // Foreign Key
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
