using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IEventDal : IGenericDal<Event>
    {
        Task<Event?> GetByIdWithDetailsAsync(int id);
        Task<List<Event>> GetEventsFilteredAsync(int? categoryId, DateTime? fromDate, DateTime? toDate, bool? isFree, string? searchQuery, int page, int pageSize);
        Task<int> GetEventsCountAsync(int? categoryId, DateTime? fromDate, DateTime? toDate, bool? isFree, string? searchQuery);
    }
}
