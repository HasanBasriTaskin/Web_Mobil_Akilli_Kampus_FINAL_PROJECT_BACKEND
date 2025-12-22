namespace SMARTCAMPUS.EntityLayer.DTOs.Wallet
{
    public class PaymentResultDto
    {
        public bool IsSuccess { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? TransactionId { get; set; }
    }
}
