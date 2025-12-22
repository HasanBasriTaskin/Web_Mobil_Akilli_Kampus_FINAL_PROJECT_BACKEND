using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Event;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IEventCategoryService
    {
        Task<Response<List<EventCategoryDto>>> GetAllAsync(bool includeInactive = false);
        Task<Response<EventCategoryDto>> GetByIdAsync(int id);
        Task<Response<EventCategoryDto>> CreateAsync(string name, string? description, string? iconName);
        Task<Response<EventCategoryDto>> UpdateAsync(int id, string? name, string? description, string? iconName, bool? isActive);
        Task<Response<NoDataDto>> DeleteAsync(int id);
    }
}
