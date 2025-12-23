using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Wallet
{
    public class WalletTransactionDto
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public TransactionType Type { get; set; }
        public string TypeName => Type.ToString();
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public ReferenceType ReferenceType { get; set; }
        public string ReferenceTypeName => ReferenceType.ToString();
        public int? ReferenceId { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
