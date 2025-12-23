namespace SMARTCAMPUS.EntityLayer.DTOs.Wallet
{
    public class PaymentVerificationResultDto
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? TransactionId { get; set; }
        public decimal PaidPrice { get; set; }
        public string? UserId { get; set; }
    }
}
