namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface INotificationService
    {
        Task SendPasswordResetEmailAsync(string to, string resetLink);
    }
}
