using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Cafeteria;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface ICafeteriaService
    {
        Task<Response<List<CafeteriaDto>>> GetAllAsync(bool includeInactive = false);
        Task<Response<CafeteriaDto>> GetByIdAsync(int id);
        Task<Response<CafeteriaDto>> CreateAsync(CafeteriaCreateDto dto);
        Task<Response<CafeteriaDto>> UpdateAsync(int id, CafeteriaUpdateDto dto);
        Task<Response<NoDataDto>> DeleteAsync(int id);
    }
}
