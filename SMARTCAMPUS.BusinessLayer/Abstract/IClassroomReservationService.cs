using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IClassroomReservationService
    {
        // Listeleme
        Task<Response<List<ClassroomReservationDto>>> GetMyReservationsAsync(string userId);
        Task<Response<List<ClassroomReservationDto>>> GetPendingReservationsAsync(); // Admin
        Task<Response<List<ClassroomAvailabilityDto>>> GetClassroomAvailabilityAsync(int classroomId, DateTime date);
        
        // CRUD (Faculty)
        Task<Response<ClassroomReservationDto>> CreateReservationAsync(string userId, ClassroomReservationCreateDto dto);
        Task<Response<NoDataDto>> CancelReservationAsync(string userId, int reservationId);
        
        // Admin
        Task<Response<NoDataDto>> ApproveReservationAsync(string adminUserId, int reservationId, string? notes);
        Task<Response<NoDataDto>> RejectReservationAsync(string adminUserId, int reservationId, string reason);
        Task<Response<List<ClassroomReservationDto>>> GetReservationsByDateAsync(DateTime date, int? classroomId = null);
    }
}
