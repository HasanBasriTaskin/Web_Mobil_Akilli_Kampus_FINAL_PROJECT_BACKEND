using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class WalletTransaction : BaseEntity
    {
        public int WalletId { get; set; }
        
        [ForeignKey("WalletId")]
        public Wallet Wallet { get; set; } = null!;
        
        public TransactionType Type { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }
        
        public ReferenceType ReferenceType { get; set; }
        
        public int? ReferenceId { get; set; }
        
        [MaxLength(200)]
        public string? Description { get; set; }
        
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }
}
