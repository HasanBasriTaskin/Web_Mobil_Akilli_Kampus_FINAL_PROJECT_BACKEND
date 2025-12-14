using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IDepartmentService
    {
        Task<Response<List<DepartmentDto>>> GetDepartmentsAsync();
    }
}
