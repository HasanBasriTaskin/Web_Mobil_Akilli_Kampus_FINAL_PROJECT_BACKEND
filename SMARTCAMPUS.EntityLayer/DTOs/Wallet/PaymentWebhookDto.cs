namespace SMARTCAMPUS.EntityLayer.DTOs.Wallet
{
    public class PaymentWebhookDto
    {
        public string TransactionId { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal Amount { get; set; }
        public string UserId { get; set; } = null!;
    }
}
