namespace SMARTCAMPUS.EntityLayer.DTOs.Wallet
{
    public class TopUpResultDto
    {
        public decimal NewBalance { get; set; }
        public int TransactionId { get; set; }
        public string? PaymentUrl { get; set; }
    }
}
