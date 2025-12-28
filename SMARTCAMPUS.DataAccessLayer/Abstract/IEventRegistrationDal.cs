using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IEventRegistrationDal : IGenericDal<EventRegistration>
    {
        Task<EventRegistration?> GetByEventAndUserAsync(int eventId, string userId);
        Task<EventRegistration?> GetByQRCodeAsync(string qrCode);
        Task<List<EventRegistration>> GetByEventIdAsync(int eventId);
        Task<List<EventRegistration>> GetByUserIdAsync(string userId);
        Task<bool> IsUserRegisteredAsync(int eventId, string userId);
    }
}
