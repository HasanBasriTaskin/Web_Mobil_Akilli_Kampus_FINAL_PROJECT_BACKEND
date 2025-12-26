namespace SMARTCAMPUS.EntityLayer.DTOs.Wallet
{
    public class PaymentInitializeResultDto
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ConversationId { get; set; }
        public string? HtmlContent { get; set; }
        public string? PaymentPageUrl { get; set; }
    }
}
