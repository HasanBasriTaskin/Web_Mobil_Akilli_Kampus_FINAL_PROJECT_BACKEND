using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SMARTCAMPUS.BusinessLayer.Abstract;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class EmailService : INotificationService
    {
        private readonly IHostEnvironment _env;
        private readonly ILogger<EmailService> _logger;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public EmailService(IHostEnvironment env, ILogger<EmailService> logger, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _env = env;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetLink)
        {
            var subject = "Reset Your Password";
            var body = $"Please reset your password by clicking here: {resetLink}";

            if (_env.IsDevelopment())
            {
                // Development: Log to Console / Logger
                Console.WriteLine("================ EMAILSIMULATOR ================");
                Console.WriteLine($"To: {to}");
                Console.WriteLine($"Subject: {subject}");
                Console.WriteLine($"Body: {body}");
                Console.WriteLine("================================================");
                
                _logger.LogInformation($"[DevEmail] To: {to} | Subject: {subject} | ResetLink: {resetLink}");
                
                await Task.CompletedTask;
            }
            else
            {
                try 
                {
                    var host = _configuration["EmailSettings:Host"];
                    var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
                    var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
                    var fromEmail = _configuration["EmailSettings:FromEmail"];
                    var password = _configuration["EmailSettings:Password"];

                    using (var client = new System.Net.Mail.SmtpClient(host, port))
                    {
                        client.EnableSsl = enableSsl;
                        client.Credentials = new System.Net.NetworkCredential(fromEmail, password);
                        
                        var mailMessage = new System.Net.Mail.MailMessage
                        {
                            From = new System.Net.Mail.MailAddress(fromEmail!),
                            Subject = "Reset Your Password",
                            Body = $"Please reset your password by clicking here: <a href='{resetLink}'>{resetLink}</a>",
                            IsBodyHtml = true
                        };

                        mailMessage.To.Add(to);

                        await client.SendMailAsync(mailMessage);
                        _logger.LogInformation($"[ProdEmail] Email sent to {to}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[ProdEmail Error] Failed to send email: {ex.Message}");
                }
                
            }
        }

        public async Task SendEmailVerificationAsync(string to, string verificationLink)
        {
            var subject = "Verify Your Email Account";
            var body = $"Please verify your account by clicking the link below: {verificationLink}";

            if (_env.IsDevelopment())
            {
                // Development: Log to Console / Logger
                Console.WriteLine("================ EMAILSIMULATOR ================");
                Console.WriteLine($"To: {to}");
                Console.WriteLine($"Subject: {subject}");
                Console.WriteLine($"Body: {body}");
                Console.WriteLine("================================================");
                
                _logger.LogInformation($"[DevEmail] To: {to} | Subject: {subject} | VerifyLink: {verificationLink}");
                
                await Task.CompletedTask;
            }
            else
            {
                // Production: Real SMTP
                try 
                {
                    var host = _configuration["EmailSettings:Host"];
                    var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
                    var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
                    var fromEmail = _configuration["EmailSettings:FromEmail"];
                    var password = _configuration["EmailSettings:Password"];

                    using (var client = new System.Net.Mail.SmtpClient(host, port))
                    {
                        client.EnableSsl = enableSsl;
                        client.Credentials = new System.Net.NetworkCredential(fromEmail, password);
                        
                        var mailMessage = new System.Net.Mail.MailMessage
                        {
                            From = new System.Net.Mail.MailAddress(fromEmail!),
                            Subject = subject,
                            Body = $"Please verify your account by clicking here: <a href='{verificationLink}'>{verificationLink}</a>",
                            IsBodyHtml = true
                        };

                        mailMessage.To.Add(to);

                        await client.SendMailAsync(mailMessage);
                        _logger.LogInformation($"[ProdEmail] Verification Email sent to {to}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[ProdEmail Error] Failed to send verification email: {ex.Message}");
                }
                
                await Task.CompletedTask;
            }
        }
    }
}
