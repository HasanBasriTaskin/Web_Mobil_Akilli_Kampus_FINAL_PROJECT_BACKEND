using Microsoft.AspNetCore.Identity;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Reservation;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class MealReservationManager : IMealReservationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQRCodeService _qrCodeService;
        private readonly IWalletService _walletService;
        private readonly UserManager<User> _userManager;

        public MealReservationManager(
            IUnitOfWork unitOfWork,
            IQRCodeService qrCodeService,
            IWalletService walletService,
            UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _qrCodeService = qrCodeService;
            _walletService = walletService;
            _userManager = userManager;
        }

        public async Task<Response<MealReservationDto>> CreateReservationAsync(string userId, MealReservationCreateDto dto)
        {
            if (dto.Date.Date < DateTime.UtcNow.Date)
                return Response<MealReservationDto>.Fail("Geçmiş tarih için rezervasyon yapılamaz", 400);

            if (dto.Date.DayOfWeek == DayOfWeek.Saturday || dto.Date.DayOfWeek == DayOfWeek.Sunday)
                return Response<MealReservationDto>.Fail("Hafta sonu için rezervasyon yapılamaz", 400);

            var existingReservation = await _unitOfWork.MealReservations.ExistsForUserDateMealTypeAsync(userId, dto.Date, dto.MealType);
            if (existingReservation)
                return Response<MealReservationDto>.Fail("Bu öğün için zaten bir rezervasyonunuz var", 400);

            var menu = await _unitOfWork.MealMenus.GetByIdWithDetailsAsync(dto.MenuId);
            if (menu == null || !menu.IsActive || !menu.IsPublished)
                return Response<MealReservationDto>.Fail("Geçersiz veya yayınlanmamış menü", 400);

            if (menu.Date.Date != dto.Date.Date || menu.MealType != dto.MealType || menu.CafeteriaId != dto.CafeteriaId)
                return Response<MealReservationDto>.Fail("Menü bilgileri uyuşmuyor", 400);

            var student = await _unitOfWork.Students.GetByUserIdAsync(userId);
            bool isFree = false;

            if (student != null && student.HasScholarship)
            {
                var todayReservationCount = await _unitOfWork.MealReservations.GetDailyReservationCountAsync(userId, dto.Date);
                if (todayReservationCount < student.DailyMealQuota)
                {
                    isFree = true;
                }
            }

            if (!isFree && menu.Price > 0)
            {
                var deductResult = await _walletService.DeductAsync(
                    userId,
                    menu.Price,
                    ReferenceType.MealReservation,
                    null,
                    $"Yemek rezervasyonu - {menu.Cafeteria.Name} - {dto.Date:dd.MM.yyyy} {dto.MealType}");

                if (!deductResult.IsSuccessful)
                    return Response<MealReservationDto>.Fail(deductResult.Errors?.FirstOrDefault() ?? "Ödeme başarısız", 400);
            }

            var reservation = new MealReservation
            {
                UserId = userId,
                MenuId = dto.MenuId,
                CafeteriaId = dto.CafeteriaId,
                MealType = dto.MealType,
                Date = dto.Date.Date,
                QRCode = _qrCodeService.GenerateQRCode("MEAL", 0),
                Status = MealReservationStatus.Reserved,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.MealReservations.AddAsync(reservation);
            await _unitOfWork.CommitAsync();

            reservation.QRCode = _qrCodeService.GenerateQRCode("MEAL", reservation.Id);
            _unitOfWork.MealReservations.Update(reservation);
            await _unitOfWork.CommitAsync();

            var user = await _userManager.FindByIdAsync(userId);

            return Response<MealReservationDto>.Success(new MealReservationDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = user?.UserName ?? "Bilinmeyen",
                MenuId = reservation.MenuId,
                CafeteriaId = reservation.CafeteriaId,
                CafeteriaName = menu.Cafeteria.Name,
                MealType = reservation.MealType,
                Date = reservation.Date,
                QRCode = reservation.QRCode,
                Status = reservation.Status,
                CreatedAt = reservation.CreatedDate
            }, 201);
        }

        public async Task<Response<NoDataDto>> CancelReservationAsync(string userId, int reservationId)
        {
            var reservations = await _unitOfWork.MealReservations.GetByUserAsync(userId);
            var reservation = reservations.FirstOrDefault(r => r.Id == reservationId);

            if (reservation == null)
                return Response<NoDataDto>.Fail("Rezervasyon bulunamadı", 404);

            if (reservation.Status != MealReservationStatus.Reserved)
                return Response<NoDataDto>.Fail("Bu rezervasyon iptal edilemez", 400);

            var mealStartTime = reservation.MealType switch
            {
                MealType.Breakfast => new TimeSpan(7, 0, 0),
                MealType.Lunch => new TimeSpan(11, 30, 0),
                MealType.Dinner => new TimeSpan(17, 0, 0),
                _ => new TimeSpan(12, 0, 0)
            };
            var mealDateTime = reservation.Date.Add(mealStartTime);
            var cancellationDeadline = mealDateTime.AddHours(-2);

            if (DateTime.UtcNow > cancellationDeadline)
                return Response<NoDataDto>.Fail("İptal süresi geçmiş (öğün başlamadan 2 saat önce)", 400);

            if (reservation.Menu.Price > 0)
            {
                await _walletService.RefundAsync(
                    userId,
                    reservation.Menu.Price,
                    ReferenceType.MealReservation,
                    reservation.Id,
                    $"Yemek iadesi - {reservation.Menu.Cafeteria.Name} - {reservation.Date:dd.MM.yyyy}");
            }

            reservation.Status = MealReservationStatus.Cancelled;
            reservation.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.MealReservations.Update(reservation);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<List<MealReservationListDto>>> GetMyReservationsAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var reservations = await _unitOfWork.MealReservations.GetByUserAsync(userId, fromDate, toDate);

            var dtos = reservations.Select(r => new MealReservationListDto
            {
                Id = r.Id,
                CafeteriaName = r.Menu?.Cafeteria?.Name ?? "Bilinmeyen",
                MealType = r.MealType,
                Date = r.Date,
                Status = r.Status,
                QRCode = r.QRCode
            }).ToList();

            return Response<List<MealReservationListDto>>.Success(dtos, 200);
        }

        public async Task<Response<MealReservationDto>> GetReservationByIdAsync(string userId, int reservationId)
        {
            var reservations = await _unitOfWork.MealReservations.GetByUserAsync(userId);
            var reservation = reservations.FirstOrDefault(r => r.Id == reservationId);

            if (reservation == null)
                return Response<MealReservationDto>.Fail("Rezervasyon bulunamadı", 404);

            var user = await _userManager.FindByIdAsync(userId);

            return Response<MealReservationDto>.Success(new MealReservationDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = user?.UserName ?? "Bilinmeyen",
                MenuId = reservation.MenuId,
                CafeteriaId = reservation.CafeteriaId,
                CafeteriaName = reservation.Menu?.Cafeteria?.Name ?? "Bilinmeyen",
                MealType = reservation.MealType,
                Date = reservation.Date,
                QRCode = reservation.QRCode,
                Status = reservation.Status,
                UsedAt = reservation.UsedAt,
                CreatedAt = reservation.CreatedDate
            }, 200);
        }

        public async Task<Response<MealScanResultDto>> ScanQRCodeAsync(string qrCode)
        {
            var reservation = await _unitOfWork.MealReservations.GetByQRCodeAsync(qrCode);

            if (reservation == null)
                return Response<MealScanResultDto>.Success(new MealScanResultDto
                {
                    IsValid = false,
                    Message = "Geçersiz QR kod"
                }, 200);

            if (reservation.Status == MealReservationStatus.Used)
                return Response<MealScanResultDto>.Success(new MealScanResultDto
                {
                    ReservationId = reservation.Id,
                    UserName = reservation.User?.UserName ?? "Bilinmeyen",
                    CafeteriaName = reservation.Menu?.Cafeteria?.Name ?? "Bilinmeyen",
                    MealType = reservation.MealType,
                    Date = reservation.Date,
                    IsValid = false,
                    Message = "Bu rezervasyon zaten kullanılmış"
                }, 200);

            if (reservation.Status == MealReservationStatus.Cancelled)
                return Response<MealScanResultDto>.Success(new MealScanResultDto
                {
                    ReservationId = reservation.Id,
                    IsValid = false,
                    Message = "Bu rezervasyon iptal edilmiş"
                }, 200);

            if (reservation.Status == MealReservationStatus.Expired)
                return Response<MealScanResultDto>.Success(new MealScanResultDto
                {
                    ReservationId = reservation.Id,
                    IsValid = false,
                    Message = "Bu rezervasyonun süresi dolmuş"
                }, 200);

            if (reservation.Date.Date != DateTime.UtcNow.Date)
                return Response<MealScanResultDto>.Success(new MealScanResultDto
                {
                    ReservationId = reservation.Id,
                    UserName = reservation.User?.UserName ?? "Bilinmeyen",
                    CafeteriaName = reservation.Menu?.Cafeteria?.Name ?? "Bilinmeyen",
                    MealType = reservation.MealType,
                    Date = reservation.Date,
                    IsValid = false,
                    Message = $"Bu rezervasyon {reservation.Date:dd.MM.yyyy} tarihi için"
                }, 200);

            reservation.Status = MealReservationStatus.Used;
            reservation.UsedAt = DateTime.UtcNow;
            reservation.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.MealReservations.Update(reservation);
            await _unitOfWork.CommitAsync();

            return Response<MealScanResultDto>.Success(new MealScanResultDto
            {
                ReservationId = reservation.Id,
                UserName = reservation.User?.UserName ?? "Bilinmeyen",
                CafeteriaName = reservation.Menu?.Cafeteria?.Name ?? "Bilinmeyen",
                MealType = reservation.MealType,
                Date = reservation.Date,
                IsValid = true,
                Message = "Başarılı! Afiyet olsun."
            }, 200);
        }

        public async Task<Response<MealReservationDto>> GetReservationByQRAsync(string qrCode)
        {
            var reservation = await _unitOfWork.MealReservations.GetByQRCodeAsync(qrCode);

            if (reservation == null)
                return Response<MealReservationDto>.Fail("Rezervasyon bulunamadı", 404);

            return Response<MealReservationDto>.Success(new MealReservationDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = reservation.User?.UserName ?? "Bilinmeyen",
                MenuId = reservation.MenuId,
                CafeteriaId = reservation.CafeteriaId,
                CafeteriaName = reservation.Menu?.Cafeteria?.Name ?? "Bilinmeyen",
                MealType = reservation.MealType,
                Date = reservation.Date,
                QRCode = reservation.QRCode,
                Status = reservation.Status,
                UsedAt = reservation.UsedAt,
                CreatedAt = reservation.CreatedDate
            }, 200);
        }

        public async Task<Response<List<MealReservationDto>>> GetReservationsByDateAsync(DateTime date, int? cafeteriaId = null, MealType? mealType = null)
        {
            var reservations = await _unitOfWork.MealReservations.GetByDateAsync(date, cafeteriaId, mealType);

            var dtos = reservations.Select(r => new MealReservationDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User?.UserName ?? "Bilinmeyen",
                MenuId = r.MenuId,
                CafeteriaId = r.CafeteriaId,
                CafeteriaName = r.Menu?.Cafeteria?.Name ?? "Bilinmeyen",
                MealType = r.MealType,
                Date = r.Date,
                QRCode = r.QRCode,
                Status = r.Status,
                UsedAt = r.UsedAt,
                CreatedAt = r.CreatedDate
            }).ToList();

            return Response<List<MealReservationDto>>.Success(dtos, 200);
        }

        public async Task<Response<NoDataDto>> ExpireOldReservationsAsync()
        {
            var expiredReservations = await _unitOfWork.MealReservations.GetExpiredReservationsAsync();

            foreach (var reservation in expiredReservations)
            {
                reservation.Status = MealReservationStatus.Expired;
                reservation.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.MealReservations.Update(reservation);
            }

            await _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Success(200);
        }
    }
}
