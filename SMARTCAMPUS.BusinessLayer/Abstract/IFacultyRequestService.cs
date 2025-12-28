using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.FacultyRequest;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IFacultyRequestService
    {
        /// <summary>
        /// Akademisyenin bölümündeki uygun dersleri listeler
        /// </summary>
        Task<Response<IEnumerable<AvailableSectionDto>>> GetAvailableSectionsAsync(int facultyId);

        /// <summary>
        /// Akademisyen ders alma isteği gönderir
        /// </summary>
        Task<Response<FacultyRequestDto>> RequestSectionAsync(int facultyId, CreateFacultyRequestDto dto);

        /// <summary>
        /// Akademisyenin kendi isteklerini listeler
        /// </summary>
        Task<Response<IEnumerable<FacultyRequestDto>>> GetMyRequestsAsync(int facultyId);

        /// <summary>
        /// Admin: Tüm bekleyen istekleri listeler
        /// </summary>
        Task<Response<IEnumerable<FacultyRequestDto>>> GetAllPendingRequestsAsync();

        /// <summary>
        /// Admin: İsteği onaylar
        /// </summary>
        Task<Response<NoDataDto>> ApproveRequestAsync(int requestId, string adminId, string? note);

        /// <summary>
        /// Admin: İsteği reddeder
        /// </summary>
        Task<Response<NoDataDto>> RejectRequestAsync(int requestId, string adminId, string? note);
    }
}
