using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Reservation;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class MealReservationManager : IMealReservationService
    {
        private readonly CampusContext _context;
        private readonly IQRCodeService _qrCodeService;
        private readonly IWalletService _walletService;

        public MealReservationManager(CampusContext context, IQRCodeService qrCodeService, IWalletService walletService)
        {
            _context = context;
            _qrCodeService = qrCodeService;
            _walletService = walletService;
        }

        public async Task<Response<MealReservationDto>> CreateReservationAsync(string userId, MealReservationCreateDto dto)
        {
            // Geçmiş tarih kontrolü
            if (dto.Date.Date < DateTime.UtcNow.Date)
                return Response<MealReservationDto>.Fail("Geçmiş tarih için rezervasyon yapılamaz", 400);

            // Hafta sonu kontrolü
            if (dto.Date.DayOfWeek == DayOfWeek.Saturday || dto.Date.DayOfWeek == DayOfWeek.Sunday)
                return Response<MealReservationDto>.Fail("Hafta sonu için rezervasyon yapılamaz", 400);

            // Aynı gün/öğün için mevcut rezervasyon kontrolü
            var existingReservation = await _context.MealReservations
                .AnyAsync(r => r.UserId == userId && 
                              r.Date.Date == dto.Date.Date && 
                              r.MealType == dto.MealType &&
                              r.Status == MealReservationStatus.Reserved);
            if (existingReservation)
                return Response<MealReservationDto>.Fail("Bu öğün için zaten bir rezervasyonunuz var", 400);

            // Menü kontrolü
            var menu = await _context.MealMenus
                .Include(m => m.Cafeteria)
                .FirstOrDefaultAsync(m => m.Id == dto.MenuId && m.IsActive && m.IsPublished);
            if (menu == null)
                return Response<MealReservationDto>.Fail("Geçersiz veya yayınlanmamış menü", 400);

            // Menü tarihi ve öğünü kontrolü
            if (menu.Date.Date != dto.Date.Date || menu.MealType != dto.MealType || menu.CafeteriaId != dto.CafeteriaId)
                return Response<MealReservationDto>.Fail("Menü bilgileri uyuşmuyor", 400);

            // Burs kontrolü
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            bool isFree = false;

            if (student != null && student.HasScholarship)
            {
                // Günlük kota kontrolü
                var todayReservationCount = await _context.MealReservations
                    .CountAsync(r => r.UserId == userId && 
                                    r.Date.Date == dto.Date.Date && 
                                    r.Status != MealReservationStatus.Cancelled);
                
                if (todayReservationCount < student.DailyMealQuota)
                {
                    isFree = true; // Burslu ve kota dahilinde, ücretsiz
                }
            }

            // Ücretli ise bakiye kontrolü ve ödeme
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

            // Rezervasyon oluştur
            var reservation = new MealReservation
            {
                UserId = userId,
                MenuId = dto.MenuId,
                CafeteriaId = dto.CafeteriaId,
                MealType = dto.MealType,
                Date = dto.Date.Date,
                QRCode = _qrCodeService.GenerateQRCode("MEAL", 0), // Geçici, aşağıda güncellenecek
                Status = MealReservationStatus.Reserved,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _context.MealReservations.AddAsync(reservation);
            await _context.SaveChangesAsync();

            // QR kodu güncelle (artık ID var)
            reservation.QRCode = _qrCodeService.GenerateQRCode("MEAL", reservation.Id);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

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
            var reservation = await _context.MealReservations
                .Include(r => r.Menu)
                    .ThenInclude(m => m.Cafeteria)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);

            if (reservation == null)
                return Response<NoDataDto>.Fail("Rezervasyon bulunamadı", 404);

            if (reservation.Status != MealReservationStatus.Reserved)
                return Response<NoDataDto>.Fail("Bu rezervasyon iptal edilemez", 400);

            // İptal süresi kontrolü (örn: öğün başlamadan 2 saat önce)
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

            // İade işlemi
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
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<List<MealReservationListDto>>> GetMyReservationsAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.MealReservations
                .Include(r => r.Cafeteria)
                .Where(r => r.UserId == userId)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(r => r.Date >= fromDate.Value.Date);
            if (toDate.HasValue)
                query = query.Where(r => r.Date <= toDate.Value.Date);

            var reservations = await query
                .OrderByDescending(r => r.Date)
                .ThenBy(r => r.MealType)
                .Select(r => new MealReservationListDto
                {
                    Id = r.Id,
                    CafeteriaName = r.Cafeteria.Name,
                    MealType = r.MealType,
                    Date = r.Date,
                    Status = r.Status,
                    QRCode = r.QRCode
                })
                .ToListAsync();

            return Response<List<MealReservationListDto>>.Success(reservations, 200);
        }

        public async Task<Response<MealReservationDto>> GetReservationByIdAsync(string userId, int reservationId)
        {
            var reservation = await _context.MealReservations
                .Include(r => r.Cafeteria)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);

            if (reservation == null)
                return Response<MealReservationDto>.Fail("Rezervasyon bulunamadı", 404);

            return Response<MealReservationDto>.Success(MapToDto(reservation), 200);
        }

        public async Task<Response<MealScanResultDto>> ScanQRCodeAsync(string qrCode)
        {
            var reservation = await _context.MealReservations
                .Include(r => r.Cafeteria)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.QRCode == qrCode);

            if (reservation == null)
                return Response<MealScanResultDto>.Success(new MealScanResultDto
                {
                    IsValid = false,
                    Message = "Geçersiz QR kod"
                }, 200);

            // Durum kontrolü
            if (reservation.Status == MealReservationStatus.Used)
                return Response<MealScanResultDto>.Success(new MealScanResultDto
                {
                    ReservationId = reservation.Id,
                    UserName = reservation.User?.UserName ?? "Bilinmeyen",
                    CafeteriaName = reservation.Cafeteria.Name,
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

            // Tarih kontrolü
            if (reservation.Date.Date != DateTime.UtcNow.Date)
                return Response<MealScanResultDto>.Success(new MealScanResultDto
                {
                    ReservationId = reservation.Id,
                    UserName = reservation.User?.UserName ?? "Bilinmeyen",
                    CafeteriaName = reservation.Cafeteria.Name,
                    MealType = reservation.MealType,
                    Date = reservation.Date,
                    IsValid = false,
                    Message = $"Bu rezervasyon {reservation.Date:dd.MM.yyyy} tarihi için"
                }, 200);

            // Rezervasyonu kullanıldı olarak işaretle
            reservation.Status = MealReservationStatus.Used;
            reservation.UsedAt = DateTime.UtcNow;
            reservation.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<MealScanResultDto>.Success(new MealScanResultDto
            {
                ReservationId = reservation.Id,
                UserName = reservation.User?.UserName ?? "Bilinmeyen",
                CafeteriaName = reservation.Cafeteria.Name,
                MealType = reservation.MealType,
                Date = reservation.Date,
                IsValid = true,
                Message = "Başarılı! Afiyet olsun."
            }, 200);
        }

        public async Task<Response<MealReservationDto>> GetReservationByQRAsync(string qrCode)
        {
            var reservation = await _context.MealReservations
                .Include(r => r.Cafeteria)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.QRCode == qrCode);

            if (reservation == null)
                return Response<MealReservationDto>.Fail("Rezervasyon bulunamadı", 404);

            return Response<MealReservationDto>.Success(MapToDto(reservation), 200);
        }

        public async Task<Response<List<MealReservationDto>>> GetReservationsByDateAsync(DateTime date, int? cafeteriaId = null, MealType? mealType = null)
        {
            var query = _context.MealReservations
                .Include(r => r.Cafeteria)
                .Include(r => r.User)
                .Where(r => r.Date.Date == date.Date)
                .AsQueryable();

            if (cafeteriaId.HasValue)
                query = query.Where(r => r.CafeteriaId == cafeteriaId.Value);

            if (mealType.HasValue)
                query = query.Where(r => r.MealType == mealType.Value);

            var reservations = await query
                .OrderBy(r => r.MealType)
                .ThenBy(r => r.CreatedDate)
                .Select(r => MapToDto(r))
                .ToListAsync();

            return Response<List<MealReservationDto>>.Success(reservations, 200);
        }

        public async Task<Response<NoDataDto>> ExpireOldReservationsAsync()
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var expiredReservations = await _context.MealReservations
                .Where(r => r.Date.Date <= yesterday && r.Status == MealReservationStatus.Reserved)
                .ToListAsync();

            foreach (var reservation in expiredReservations)
            {
                reservation.Status = MealReservationStatus.Expired;
                reservation.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Response<NoDataDto>.Success(200);
        }

        private static MealReservationDto MapToDto(MealReservation r)
        {
            return new MealReservationDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User?.UserName ?? "Bilinmeyen",
                MenuId = r.MenuId,
                CafeteriaId = r.CafeteriaId,
                CafeteriaName = r.Cafeteria?.Name ?? "Bilinmeyen",
                MealType = r.MealType,
                Date = r.Date,
                QRCode = r.QRCode,
                Status = r.Status,
                UsedAt = r.UsedAt,
                CreatedAt = r.CreatedDate
            };
        }
    }
}
