using Microsoft.Extensions.Logging;
using SMARTCAMPUS.BusinessLayer.Abstract;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class EmailService : INotificationService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendPasswordResetEmailAsync(string to, string resetLink)
        {
            var subject = "Reset Your Password";
            var body = $"Please reset your password by clicking here: {resetLink}";

            Console.WriteLine("================ EMAIL SIMULATOR ================");
            Console.WriteLine($"To: {to}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body: {body}");
            Console.WriteLine("=================================================");
            
            _logger.LogInformation("[Email] To: {To} | Subject: {Subject} | ResetLink: {ResetLink}", to, subject, resetLink);
            
            return Task.CompletedTask;
        }

        public Task SendEmailVerificationAsync(string to, string verificationLink)
        {
            var subject = "Verify Your Email Account";
            var body = $"Please verify your account by clicking the link below: {verificationLink}";

            Console.WriteLine("================ EMAIL SIMULATOR ================");
            Console.WriteLine($"To: {to}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body: {body}");
            Console.WriteLine("=================================================");
            
            _logger.LogInformation("[Email] To: {To} | Subject: {Subject} | VerifyLink: {VerifyLink}", to, subject, verificationLink);
            
            return Task.CompletedTask;
        }
    }
}
