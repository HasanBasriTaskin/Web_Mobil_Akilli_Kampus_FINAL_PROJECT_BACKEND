using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class RefreshToken : BaseEntity
    {
        // Id inherited
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        
        // CreatedDate inherited (was CreatedAt)
        public string? CreatedByIp { get; set; }
        
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReasonRevoked { get; set; }
        
        // BaseEntity has IsActive (stored boolean).
        // This computed property checks validity based on expiry/revocation.
        public bool IsValid => Revoked == null && !IsExpired;
        
        // Foreign Key
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
