using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IEventWaitlistDal : IGenericDal<EventWaitlist>
    {
        Task<EventWaitlist?> GetByEventAndUserAsync(int eventId, string userId);
        Task<EventWaitlist?> GetNextInQueueAsync(int eventId);
        Task<List<EventWaitlist>> GetByEventIdAsync(int eventId);
        Task<bool> IsUserInWaitlistAsync(int eventId, string userId);
        Task<int> GetMaxPositionAsync(int eventId);
    }
}
