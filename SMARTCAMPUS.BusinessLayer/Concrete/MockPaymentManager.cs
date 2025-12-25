using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Wallet;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    /// <summary>
    /// Mock Ödeme Servisi - Eğitim/Demo amaçlı
    /// 
    /// Test Kartları:
    /// ┌──────────────────────────┬───────┬─────────┬────────────────────────────┐
    /// │ Kart Numarası            │ CVV   │ Tarih   │ Sonuç                      │
    /// ├──────────────────────────┼───────┼─────────┼────────────────────────────┤
    /// │ 1234-5678-1234-5678      │ 123   │ 01/26   │ ✅ Başarılı                │
    /// │ 0000-0000-0000-0000      │ 000   │ 01/26   │ ❌ Yetersiz bakiye         │
    /// │ 9999-9999-9999-9999      │ 999   │ 01/26   │ ❌ Kart bloke              │
    /// │ 1111-1111-1111-1111      │ 111   │ 01/26   │ ❌ Kart süresi dolmuş      │
    /// │ Diğer                    │ -     │ -       │ ❌ Geçersiz kart           │
    /// └──────────────────────────┴───────┴─────────┴────────────────────────────┘
    /// </summary>
    public class MockPaymentManager : IMockPaymentService
    {
        // Test Kartları
        private static readonly Dictionary<string, TestCardInfo> TestCards = new()
        {
            { "1234-5678-1234-5678", new TestCardInfo("123", "01/26", true, null, null) },
            { "0000-0000-0000-0000", new TestCardInfo("000", "01/26", false, "INSUFFICIENT_FUNDS", "Yetersiz bakiye") },
            { "9999-9999-9999-9999", new TestCardInfo("999", "01/26", false, "CARD_BLOCKED", "Kart bloke edilmiş") },
            { "1111-1111-1111-1111", new TestCardInfo("111", "01/26", false, "CARD_EXPIRED", "Kart süresi dolmuş") }
        };

        public PaymentResultDto ProcessPayment(PaymentDto dto)
        {
            // Kart numarasını normalize et (tire'leri kaldır ve yeniden ekle)
            var normalizedCardNumber = NormalizeCardNumber(dto.CardNumber);

            // Test kartı kontrolü
            if (!TestCards.TryGetValue(normalizedCardNumber, out var testCard))
            {
                return new PaymentResultDto
                {
                    IsSuccess = false,
                    ErrorCode = "INVALID_CARD",
                    ErrorMessage = "Geçersiz kart numarası"
                };
            }

            // CVV kontrolü
            if (dto.CVV != testCard.CVV)
            {
                return new PaymentResultDto
                {
                    IsSuccess = false,
                    ErrorCode = "INVALID_CVV",
                    ErrorMessage = "Geçersiz güvenlik kodu (CVV)"
                };
            }

            // Son kullanma tarihi kontrolü
            if (dto.ExpiryDate != testCard.ExpiryDate)
            {
                return new PaymentResultDto
                {
                    IsSuccess = false,
                    ErrorCode = "INVALID_EXPIRY",
                    ErrorMessage = "Geçersiz son kullanma tarihi"
                };
            }

            // Tutar kontrolü
            if (dto.Amount <= 0)
            {
                return new PaymentResultDto
                {
                    IsSuccess = false,
                    ErrorCode = "INVALID_AMOUNT",
                    ErrorMessage = "Geçersiz tutar"
                };
            }

            // Test kartının sonucunu döndür
            if (testCard.IsSuccess)
            {
                return new PaymentResultDto
                {
                    IsSuccess = true,
                    TransactionId = $"TXN-{Guid.NewGuid():N}"[..20].ToUpper()
                };
            }
            else
            {
                return new PaymentResultDto
                {
                    IsSuccess = false,
                    ErrorCode = testCard.ErrorCode,
                    ErrorMessage = testCard.ErrorMessage
                };
            }
        }

        private static string NormalizeCardNumber(string cardNumber)
        {
            // Boşluk ve tire'leri kaldır
            var digits = cardNumber.Replace(" ", "").Replace("-", "");
            
            // 16 haneli değilse olduğu gibi döndür
            if (digits.Length != 16)
                return cardNumber;

            // Tire formatına dönüştür: XXXX-XXXX-XXXX-XXXX
            return $"{digits[..4]}-{digits[4..8]}-{digits[8..12]}-{digits[12..16]}";
        }

        private record TestCardInfo(string CVV, string ExpiryDate, bool IsSuccess, string? ErrorCode, string? ErrorMessage);
    }
}
