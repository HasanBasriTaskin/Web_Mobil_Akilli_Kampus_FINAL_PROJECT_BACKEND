using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IAnnouncementService
    {
        Task<Response<IEnumerable<AnnouncementDto>>> GetAnnouncementsAsync(string? targetAudience = null, int? departmentId = null);
        Task<Response<IEnumerable<AnnouncementDto>>> GetImportantAnnouncementsAsync();
        Task<Response<AnnouncementDto>> GetAnnouncementByIdAsync(int id);
        Task<Response<NoDataDto>> IncrementViewCountAsync(int id);
    }
}
