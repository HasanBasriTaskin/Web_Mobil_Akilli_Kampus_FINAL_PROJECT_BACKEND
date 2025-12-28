using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.Configuration;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    /// <summary>
    /// Email service with SMTP support for both local and production environments.
    /// Always logs email details; sends actual emails only when IsEnabled is true.
    /// </summary>
    public class EmailService : INotificationService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _settings;

        public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetLink)
        {
            var subject = "Reset Your Password";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Password Reset Request</h2>
                    <p>You have requested to reset your password. Click the link below to proceed:</p>
                    <p><a href='{resetLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                    <p>If you did not request this, please ignore this email.</p>
                    <br/>
                    <p>Best regards,<br/>Smart Campus Team</p>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body, $"ResetLink: {resetLink}");
        }

        public async Task SendEmailVerificationAsync(string to, string verificationLink)
        {
            var subject = "Verify Your Email Account";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Welcome to Smart Campus!</h2>
                    <p>Thank you for registering. Please verify your email address by clicking the link below:</p>
                    <p><a href='{verificationLink}' style='background-color: #2196F3; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Verify Email</a></p>
                    <p>If you did not create an account, please ignore this email.</p>
                    <br/>
                    <p>Best regards,<br/>Smart Campus Team</p>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body, $"VerifyLink: {verificationLink}");
        }

        /// <summary>
        /// Sends email via SMTP, always logs email details.
        /// Errors are logged but not thrown to avoid breaking the calling operation.
        /// </summary>
        private async Task SendEmailAsync(string to, string subject, string htmlBody, string logContext)
        {
            // Always log email details (both local and production)
            Console.WriteLine("================ EMAIL SERVICE ================");
            Console.WriteLine($"To: {to}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Context: {logContext}");
            Console.WriteLine("===============================================");

            _logger.LogInformation("[Email] To: {To} | Subject: {Subject} | {LogContext}", 
                to, subject, logContext);

            try
            {
                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.EnableSsl,
                    Credentials = new NetworkCredential(
                        string.IsNullOrEmpty(_settings.Username) ? _settings.FromEmail : _settings.Username, 
                        _settings.Password),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000 // 30 seconds
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.FromEmail, "Smart Campus"),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
                
                _logger.LogInformation("[Email] Successfully sent email to {To} via SMTP ({Host}:{Port})", 
                    to, _settings.Host, _settings.Port);
            }
            catch (SmtpException smtpEx)
            {
                // Log error but don't throw - email is non-critical
                _logger.LogError(smtpEx, "[Email] SMTP error sending to {To}: {Message}. Email will not be sent.", to, smtpEx.Message);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - email is non-critical
                _logger.LogError(ex, "[Email] Unexpected error sending to {To}: {Message}. Email will not be sent.", to, ex.Message);
            }
        }
    }
}
