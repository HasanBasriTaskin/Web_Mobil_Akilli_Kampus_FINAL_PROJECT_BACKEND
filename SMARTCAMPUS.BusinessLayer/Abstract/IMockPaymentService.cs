using SMARTCAMPUS.EntityLayer.DTOs.Wallet;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IMockPaymentService
    {
        /// <summary>
        /// Mock ödeme işlemi - Test kartları ile çalışır
        /// </summary>
        PaymentResultDto ProcessPayment(PaymentDto dto);
    }
}
