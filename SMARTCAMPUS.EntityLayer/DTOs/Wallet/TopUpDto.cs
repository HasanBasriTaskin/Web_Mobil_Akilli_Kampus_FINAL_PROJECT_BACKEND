namespace SMARTCAMPUS.EntityLayer.DTOs.Wallet
{
    public class TopUpDto
    {
        public decimal Amount { get; set; }
    }

    public class WalletTopUpDto
    {
        public string CardNumber { get; set; } = null!;
        public string CVV { get; set; } = null!;
        public string ExpiryDate { get; set; } = null!;
        public decimal Amount { get; set; }
    }
}

