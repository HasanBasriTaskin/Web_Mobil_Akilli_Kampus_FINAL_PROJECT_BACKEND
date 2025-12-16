using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IExcuseRequestService
    {
        Task<Response<ExcuseRequestDto>> CreateExcuseRequestAsync(int studentId, ExcuseRequestCreateDto createDto);
        Task<Response<IEnumerable<ExcuseRequestDto>>> GetExcuseRequestsAsync(string? instructorId = null);
        Task<Response<NoDataDto>> ApproveExcuseRequestAsync(int requestId, string instructorId, string? notes = null);
        Task<Response<NoDataDto>> RejectExcuseRequestAsync(int requestId, string instructorId, string? notes = null);
    }
}

