using FluentValidation;
using SMARTCAMPUS.EntityLayer.DTOs.Wallet;

namespace SMARTCAMPUS.BusinessLayer.ValidationRules.Wallet
{
    public class WalletTopUpValidator : AbstractValidator<WalletTopUpDto>
    {
        public WalletTopUpValidator()
        {
            RuleFor(x => x.CardNumber)
                .NotEmpty()
                .WithMessage("Kart numarası zorunludur")
                .Length(16)
                .WithMessage("Kart numarası 16 haneli olmalıdır")
                .Matches(@"^\d+$")
                .WithMessage("Kart numarası sadece rakamlardan oluşmalıdır");

            RuleFor(x => x.CVV)
                .NotEmpty()
                .WithMessage("CVV zorunludur")
                .Length(3, 4)
                .WithMessage("CVV 3 veya 4 haneli olmalıdır")
                .Matches(@"^\d+$")
                .WithMessage("CVV sadece rakamlardan oluşmalıdır");

            RuleFor(x => x.ExpiryDate)
                .NotEmpty()
                .WithMessage("Son kullanma tarihi zorunludur")
                .Matches(@"^(0[1-9]|1[0-2])\/([0-9]{2})$")
                .WithMessage("Son kullanma tarihi MM/YY formatında olmalıdır");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Tutar 0'dan büyük olmalıdır")
                .LessThanOrEqualTo(10000)
                .WithMessage("Tek seferde en fazla 10.000 TL yüklenebilir");
        }
    }
}
