using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Wallet : BaseEntity
    {
        [Required]
        public string UserId { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;
        
        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "TRY";
        
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    }
}
