namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IQRCodeService
    {
        /// <summary>
        /// Generates a unique QR code string for meals/events
        /// </summary>
        /// <param name="prefix">Prefix for the QR code (e.g., "MEAL", "EVENT")</param>
        /// <param name="referenceId">Reference ID (e.g., reservation ID, registration ID)</param>
        /// <returns>Unique QR code string</returns>
        string GenerateQRCode(string prefix, int referenceId);

        /// <summary>
        /// Validates a QR code format
        /// </summary>
        /// <param name="qrCode">QR code to validate</param>
        /// <param name="expectedPrefix">Expected prefix</param>
        /// <returns>True if valid format</returns>
        bool ValidateQRCodeFormat(string qrCode, string expectedPrefix);

        /// <summary>
        /// Extracts reference ID from QR code
        /// </summary>
        /// <param name="qrCode">QR code string</param>
        /// <returns>Reference ID or null if invalid</returns>
        int? ExtractReferenceId(string qrCode);

        /// <summary>
        /// Generates QR code image as byte array
        /// </summary>
        /// <param name="qrCode">QR code content</param>
        /// <returns>PNG image bytes</returns>
        byte[] GenerateQRCodeImage(string qrCode);
    }
}
