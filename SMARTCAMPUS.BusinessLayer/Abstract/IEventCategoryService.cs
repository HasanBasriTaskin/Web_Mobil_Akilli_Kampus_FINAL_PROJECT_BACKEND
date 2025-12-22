using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Event;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IEventCategoryService
    {
        Task<Response<List<EventCategoryDto>>> GetAllAsync(bool includeInactive = false);
        Task<Response<EventCategoryDto>> GetByIdAsync(int id);
        Task<Response<EventCategoryDto>> CreateAsync(EventCategoryCreateDto dto);
        Task<Response<EventCategoryDto>> UpdateAsync(int id, EventCategoryUpdateDto dto);
        Task<Response<NoDataDto>> DeleteAsync(int id);
    }
}
