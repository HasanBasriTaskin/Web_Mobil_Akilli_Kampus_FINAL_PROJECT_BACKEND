using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IEventCategoryDal : IGenericDal<EventCategory>
    {
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<bool> HasActiveEventsAsync(int categoryId);
    }
}
