namespace SMARTCAMPUS.EntityLayer.Configuration
{
    /// <summary>
    /// Email settings configuration for SMTP integration.
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// SMTP server host address.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// SMTP server port (usually 587 for TLS, 465 for SSL).
        /// </summary>
        public int Port { get; set; } = 587;

        /// <summary>
        /// Enable SSL/TLS encryption.
        /// </summary>
        public bool EnableSsl { get; set; } = true;

        /// <summary>
        /// Sender email address.
        /// </summary>
        public string FromEmail { get; set; } = string.Empty;

        /// <summary>
        /// SMTP authentication username (e.g., "apikey" for SendGrid).
        /// If empty, FromEmail will be used as username.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// SMTP authentication password or API key.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
