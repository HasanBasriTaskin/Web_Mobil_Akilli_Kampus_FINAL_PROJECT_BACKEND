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

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class IyzicoPaymentManager : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly Options _options;
        private readonly IUnitOfWork _unitOfWork;

        // Callback URL yapılandırması
        // Not: Localhost'ta çalışırken webhooks çalışmayabilir, ngrok vb. gerekebilir.
        // Ancak frontend üzerinden redirect ile sonuca varılacaksa bu URL frontend olmalı.
        // Backend verification için:
        private string CallbackUrl => $"{_configuration["ClientSettings:Url"] ?? "http://localhost:3000"}/wallet/callback";

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
                    Name = user.FullName?.Split(' ').FirstOrDefault() ?? "Guest",
                    Surname = user.FullName?.Split(' ').LastOrDefault() ?? "User",
                    GsmNumber = dto.GsmNumber ?? user.PhoneNumber ?? "+905000000000",
                    Email = user.Email ?? "email@email.com",
                    IdentityNumber = "11111111110", // Zorunlu alan, mock
                    LastLoginDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    RegistrationDate = user.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    RegistrationAddress = dto.Address ?? "N/A",
                    Ip = ipAddress,
                    City = dto.City ?? "Istanbul",
                    Country = dto.Country ?? "Turkey",
                    ZipCode = "34732"
                },
                BillingAddress = new Address
                {
                    ContactName = user.FullName,
                    City = dto.City ?? "Istanbul",
                    Country = dto.Country ?? "Turkey",
                    Description = dto.Address ?? "N/A",
                    ZipCode = "34732"
                },
                ShippingAddress = new Address
                {
                    ContactName = user.FullName,
                    City = dto.City ?? "Istanbul",
                    Country = dto.Country ?? "Turkey",
                    Description = dto.Address ?? "N/A",
                    ZipCode = "34732"
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

            // Iyzipay async çağrı (Build hatasına göre Task dönüyor)
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
                return Response<PaymentInitializeResultDto>.Fail($"Iyzico Başlatma Hatası: {checkoutFormInitialize.ErrorMessage}", 400);
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
    }
}
