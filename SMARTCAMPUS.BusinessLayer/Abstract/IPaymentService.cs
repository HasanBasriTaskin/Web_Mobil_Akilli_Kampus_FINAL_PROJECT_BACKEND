using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Wallet;
using SMARTCAMPUS.BusinessLayer.Common;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IPaymentService
    {
        /// <summary>
        /// Ödeme sayfasını başlatır (Checkout Form veya 3D Secure)
        /// </summary>
        Task<Response<PaymentInitializeResultDto>> InitializePaymentAsync(string userId, IyzicoPaymentDto dto, string ipAddress);

        /// <summary>
        /// Ödeme sonucunu doğrular (Webhook veya Callback sonrası)
        /// </summary>
        Task<Response<PaymentVerificationResultDto>> VerifyPaymentAsync(string token, string conversationId);
    }
}
