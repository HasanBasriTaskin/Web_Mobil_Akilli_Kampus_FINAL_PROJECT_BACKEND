using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Configuration;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Wallet;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class IyzicoPaymentManager : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly Options _options;
        private readonly IUnitOfWork _unitOfWork;

        // Callback URL yapılandırması
        // Not: Iyzico başarılı ödemede buraya POST atar.
        // Frontend yerine Backend API'ye yönlendiriyoruz, böylece token'ı verify edip bakiyeyi ekleyebiliriz.
        // Ardından PaymentWebhookController kullanıcıyı frontend success/fail sayfasına yönlendirecek.
        private string CallbackUrl => $"{_configuration["ApiBaseUrl"] ?? "https://api.smartcampus.taskinnovation.net"}/api/v1/paymentwebhook/callback";

        public IyzicoPaymentManager(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;

            var iyzicoSection = _configuration.GetSection("IyzicoSettings");
            _options = new Options
            {
                ApiKey = iyzicoSection["ApiKey"] ?? "sandbox-api-key",
                SecretKey = iyzicoSection["SecretKey"] ?? "sandbox-secret-key",
                BaseUrl = iyzicoSection["BaseUrl"] ?? "https://sandbox-api.iyzipay.com"
            };
        }

        public async Task<Response<PaymentInitializeResultDto>> InitializePaymentAsync(string userId, IyzicoPaymentDto dto, string ipAddress)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return Response<PaymentInitializeResultDto>.Fail("Kullanıcı bulunamadı", 404);

            // Buyer zorunlu alanlarını güvenli şekilde hazırla
            string name = GetSafeName(user.FullName);
            string surname = GetSafeSurname(user.FullName);
            string gsmNumber = FormatGsmNumber(dto.GsmNumber ?? user.PhoneNumber);
            string email = !string.IsNullOrEmpty(user.Email) ? user.Email : "email@smartcampus.com";
            string identityNumber = "11111111110"; // SANDBOX: Test TC No. Prod ortamında user'dan alınmalıdır.
            string addressText = !string.IsNullOrEmpty(dto.Address) ? dto.Address : "Dijital Cüzdan Yüklemesi";
            string city = !string.IsNullOrEmpty(dto.City) ? dto.City : "Istanbul";
            string country = !string.IsNullOrEmpty(dto.Country) ? dto.Country : "Turkey";
            string zipCode = "34732"; // Varsayılan Zip

            var request = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = $"{userId}|{Guid.NewGuid()}",
                Price = dto.Amount.ToString("F2", CultureInfo.InvariantCulture),
                PaidPrice = dto.Amount.ToString("F2", CultureInfo.InvariantCulture),
                Currency = Currency.TRY.ToString(),
                BasketId = $"BASKET-{DateTime.UtcNow.Ticks}",
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = CallbackUrl,
                EnabledInstallments = new List<int> { 1 },
                Buyer = new Buyer
                {
                    Id = user.Id,
                    Name = name,
                    Surname = surname,
                    GsmNumber = gsmNumber,
                    Email = email,
                    IdentityNumber = identityNumber,
                    LastLoginDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    RegistrationDate = user.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    RegistrationAddress = addressText,
                    Ip = ipAddress,
                    City = city,
                    Country = country,
                    ZipCode = zipCode
                },
                BillingAddress = new Address
                {
                    ContactName = $"{name} {surname}",
                    City = city,
                    Country = country,
                    Description = addressText,
                    ZipCode = zipCode
                },
                ShippingAddress = new Address
                {
                    ContactName = $"{name} {surname}",
                    City = city,
                    Country = country,
                    Description = addressText,
                    ZipCode = zipCode
                }
            };

            var basketItem = new BasketItem
            {
                Id = "BI-101",
                Name = "Cüzdan Bakiyesi",
                Category1 = "Wallet",
                Category2 = "TopUp",
                ItemType = BasketItemType.VIRTUAL.ToString(),
                Price = dto.Amount.ToString("F2", CultureInfo.InvariantCulture)
            };
            request.BasketItems = new List<BasketItem> { basketItem };

            // Iyzipay async çağrı
            var checkoutFormInitialize = await CheckoutFormInitialize.Create(request, _options);


            if (checkoutFormInitialize.Status == "success")
            {
                return Response<PaymentInitializeResultDto>.Success(new PaymentInitializeResultDto
                {
                    IsSuccess = true,
                    ConversationId = request.ConversationId,
                    HtmlContent = checkoutFormInitialize.CheckoutFormContent,
                    PaymentPageUrl = checkoutFormInitialize.PaymentPageUrl
                }, 200);
            }
            else
            {
                return Response<PaymentInitializeResultDto>.Fail($"Iyzico Başlatma Hatası: {checkoutFormInitialize.ErrorMessage} | ErrorCode: {checkoutFormInitialize.ErrorCode}", 400);
            }
        }

        public async Task<Response<PaymentVerificationResultDto>> VerifyPaymentAsync(string token, string conversationId)
        {
            var request = new RetrieveCheckoutFormRequest
            {
                Token = token,
                ConversationId = conversationId
            };

            // Iyzipay async çağrı
            var checkoutForm = await CheckoutForm.Retrieve(request, _options);

            if (checkoutForm.Status == "success" && checkoutForm.PaymentStatus == "SUCCESS")
            {
                // PaidPrice'ı decimal'e çevir
                decimal.TryParse(checkoutForm.PaidPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal paidPrice);

                // Conversation ID: UserId|GUID formatindaysa parse edelim.
                var userId = conversationId?.Split('|').FirstOrDefault();
                if (string.IsNullOrEmpty(userId))
                {
                   // Fallback: PaymentItems içinde ItemId olarak saklamış olabiliriz veya Guest olabilir
                   userId = checkoutForm.PaymentItems?.FirstOrDefault()?.ItemId; 
                }
                
                return Response<PaymentVerificationResultDto>.Success(new PaymentVerificationResultDto
                {
                    IsSuccess = true,
                    TransactionId = checkoutForm.PaymentId,
                    PaidPrice = paidPrice,
                    UserId = userId
                }, 200);
            }
            
            return Response<PaymentVerificationResultDto>.Fail($"Ödeme doğrulanamadı: {checkoutForm.ErrorMessage}", 400);
        }

        #region Helpers

        private string GetSafeName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "Guest";
            var parts = fullName.Trim().Split(' ');
            return parts.Length > 0 ? parts[0] : "Guest";
        }

        private string GetSafeSurname(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "User";
            var parts = fullName.Trim().Split(' ');
            // Eğer sadece isim varsa (tek kelime), soyisim olarak "User" veya ismin aynısını kullanabiliriz.
            // Iyzico soyadı zorunlu kılar.
            if (parts.Length > 1)
            {
                return string.Join(" ", parts.Skip(1));
            }
            return "User";
        }

        private string FormatGsmNumber(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return "+905000000000";

            // Sadece rakamları al
            var digits = Regex.Replace(phone, @"[^\d]", "");

            // Türkiye formatı kontrolü
            if (digits.Length == 10) // 5XX...
                return "+90" + digits;
            if (digits.Length == 11 && digits.StartsWith("0")) // 05XX...
                return "+9" + digits;
            if (digits.Length == 12 && digits.StartsWith("90")) // 905XX...
                return "+" + digits;
            
            // Eğer none-TR veya unknown ise +90 default ile veya direk döndür (Iyzico hata verebilir)
            // Sandbox için güvenli moda alıyoruz:
            if (digits.Length < 10) return "+905000000000";
            
            return "+" + digits;
        }

        #endregion
    }
}
