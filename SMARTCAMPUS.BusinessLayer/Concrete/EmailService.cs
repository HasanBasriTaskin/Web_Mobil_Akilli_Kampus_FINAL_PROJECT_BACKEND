using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SMARTCAMPUS.BusinessLayer.Abstract;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class EmailService : INotificationService
    {
        private readonly IHostEnvironment _env;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IHostEnvironment env, ILogger<EmailService> logger)
        {
            _env = env;
            _logger = logger;
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
                // TODO: Production: Implement real SMTP or SendGrid logic here
                _logger.LogInformation($"[ProdEmail] Sending Password Reset email to {to}...");
                await Task.CompletedTask;
            }
        }
    }
}
