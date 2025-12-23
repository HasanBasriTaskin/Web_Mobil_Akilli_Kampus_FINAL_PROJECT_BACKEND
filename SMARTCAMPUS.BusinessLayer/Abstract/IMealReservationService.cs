using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Reservation;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IMealReservationService
    {
        // Kullanıcı işlemleri
        Task<Response<MealReservationDto>> CreateReservationAsync(string userId, MealReservationCreateDto dto);
        Task<Response<NoDataDto>> CancelReservationAsync(string userId, int reservationId);
        Task<Response<List<MealReservationListDto>>> GetMyReservationsAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<Response<MealReservationDto>> GetReservationByIdAsync(string userId, int reservationId);
        
        // QR Kod işlemleri (Admin/Staff)
        Task<Response<MealScanResultDto>> ScanQRCodeAsync(string qrCode);
        Task<Response<MealReservationDto>> GetReservationByQRAsync(string qrCode);
        
        // Admin işlemleri
        Task<Response<List<MealReservationDto>>> GetReservationsByDateAsync(DateTime date, int? cafeteriaId = null, MealType? mealType = null);
        Task<Response<NoDataDto>> ExpireOldReservationsAsync(); // Background job için
    }
}
