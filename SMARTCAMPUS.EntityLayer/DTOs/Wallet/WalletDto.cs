namespace SMARTCAMPUS.EntityLayer.DTOs.Wallet
{
    public class WalletDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "TRY";
        public bool IsActive { get; set; }
    }
}
