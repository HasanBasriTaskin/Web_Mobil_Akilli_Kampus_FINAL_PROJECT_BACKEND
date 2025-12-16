using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IAnnouncementDal : IGenericDal<Announcement>
    {
        Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync(string? targetAudience = null, int? departmentId = null);
        Task<IEnumerable<Announcement>> GetImportantAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetAnnouncementsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
