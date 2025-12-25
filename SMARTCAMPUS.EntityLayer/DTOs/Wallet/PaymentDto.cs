namespace SMARTCAMPUS.EntityLayer.DTOs.Wallet
{
    public class PaymentDto
    {
        public string CardNumber { get; set; } = null!;  // "1234-5678-1234-5678"
        public string CVV { get; set; } = null!;          // "123"
        public string ExpiryDate { get; set; } = null!;   // "01/26"
        public decimal Amount { get; set; }
    }
}
