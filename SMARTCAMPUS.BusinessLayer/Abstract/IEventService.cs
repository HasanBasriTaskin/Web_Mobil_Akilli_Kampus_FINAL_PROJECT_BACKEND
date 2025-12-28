using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Event;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IEventService
    {
        // Listeleme
        Task<Response<PagedResponse<EventListDto>>> GetEventsAsync(EventFilterDto filter, int page = 1, int pageSize = 20);
        Task<Response<EventDto>> GetEventByIdAsync(int id, string? userId = null);
        
        // CRUD (Admin/Organizatör)
        Task<Response<EventDto>> CreateEventAsync(string organizerId, EventCreateDto dto);
        Task<Response<EventDto>> UpdateEventAsync(string organizerId, int id, EventUpdateDto dto);
        Task<Response<NoDataDto>> DeleteEventAsync(string organizerId, int id, bool force = false);
        Task<Response<NoDataDto>> PublishEventAsync(int id);
        Task<Response<NoDataDto>> CancelEventAsync(int id, string reason);
        
        // Kayıt işlemleri
        Task<Response<EventRegistrationDto>> RegisterAsync(string userId, int eventId);
        Task<Response<NoDataDto>> CancelRegistrationAsync(string userId, int eventId);
        Task<Response<List<EventRegistrationDto>>> GetMyRegistrationsAsync(string userId);
        
        // Waitlist
        Task<Response<EventWaitlistDto>> JoinWaitlistAsync(string userId, int eventId);
        Task<Response<NoDataDto>> LeaveWaitlistAsync(string userId, int eventId);
        
        // Check-in (QR kod)
        Task<Response<EventCheckInResultDto>> CheckInAsync(string qrCode);
        
        // Admin
        Task<Response<List<EventRegistrationDto>>> GetEventRegistrationsAsync(int eventId);
    }
}
