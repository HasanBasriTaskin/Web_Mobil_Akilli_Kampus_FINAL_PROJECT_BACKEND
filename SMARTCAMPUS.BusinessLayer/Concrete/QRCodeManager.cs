using QRCoder;
using SMARTCAMPUS.BusinessLayer.Abstract;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class QRCodeManager : IQRCodeService
    {
        private const string Separator = "-";

        public string GenerateQRCode(string prefix, int referenceId)
        {
            var timestamp = DateTime.UtcNow.Ticks.ToString("X");
            var randomPart = Guid.NewGuid().ToString("N")[..8].ToUpper();
            return $"{prefix}{Separator}{referenceId}{Separator}{timestamp}{Separator}{randomPart}";
        }

        public bool ValidateQRCodeFormat(string qrCode, string expectedPrefix)
        {
            if (string.IsNullOrEmpty(qrCode))
                return false;

            var parts = qrCode.Split(Separator);
            if (parts.Length < 2)
                return false;

            return parts[0].Equals(expectedPrefix, StringComparison.OrdinalIgnoreCase);
        }

        public int? ExtractReferenceId(string qrCode)
        {
            if (string.IsNullOrEmpty(qrCode))
                return null;

            var parts = qrCode.Split(Separator);
            if (parts.Length < 2)
                return null;

            if (int.TryParse(parts[1], out var referenceId))
                return referenceId;

            return null;
        }

        public byte[] GenerateQRCodeImage(string qrCode)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(qrCode, QRCodeGenerator.ECCLevel.Q);
            using var pngQrCode = new PngByteQRCode(qrCodeData);
            return pngQrCode.GetGraphic(20);
        }
    }
}
