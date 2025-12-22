using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class ClassroomReservationManager : IClassroomReservationService
    {
        private readonly CampusContext _context;

        public ClassroomReservationManager(CampusContext context)
        {
            _context = context;
        }

        public async Task<Response<List<ClassroomReservationDto>>> GetMyReservationsAsync(string userId)
        {
            var reservations = await _context.ClassroomReservations
                .Include(r => r.Classroom)
                .Include(r => r.RequestedBy)
                .Where(r => r.RequestedByUserId == userId && r.IsActive)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .Select(r => MapToDto(r))
                .ToListAsync();

            return Response<List<ClassroomReservationDto>>.Success(reservations, 200);
        }

        public async Task<Response<List<ClassroomReservationDto>>> GetPendingReservationsAsync()
        {
            var reservations = await _context.ClassroomReservations
                .Include(r => r.Classroom)
                .Include(r => r.RequestedBy)
                .Where(r => r.Status == ReservationStatus.Pending && r.IsActive)
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .Select(r => MapToDto(r))
                .ToListAsync();

            return Response<List<ClassroomReservationDto>>.Success(reservations, 200);
        }

        public async Task<Response<List<ClassroomAvailabilityDto>>> GetClassroomAvailabilityAsync(int classroomId, DateTime date)
        {
            var classroom = await _context.Classrooms.FindAsync(classroomId);
            if (classroom == null)
                return Response<List<ClassroomAvailabilityDto>>.Fail("Sınıf bulunamadı", 404);

            var dayOfWeek = date.DayOfWeek;
            var classroomName = $"{classroom.Building} - {classroom.RoomNumber}";

            // Ders programından meşgul saatler
            var scheduleSlots = await _context.Schedules
                .Where(s => s.ClassroomId == classroomId && s.DayOfWeek == dayOfWeek && s.IsActive)
                .Select(s => new { s.StartTime, s.EndTime, Reason = "Ders" })
                .ToListAsync();

            // Onaylı rezervasyonlar
            var reservationSlots = await _context.ClassroomReservations
                .Where(r => r.ClassroomId == classroomId && 
                           r.ReservationDate.Date == date.Date && 
                           r.Status == ReservationStatus.Approved &&
                           r.IsActive)
                .Select(r => new { r.StartTime, r.EndTime, Reason = $"Rezervasyon: {r.Purpose}" })
                .ToListAsync();

            var busySlots = scheduleSlots.Concat(reservationSlots).ToList();

            // Müsait saatler (08:00 - 22:00 arası saatlik dilimler)
            var availability = new ClassroomAvailabilityDto
            {
                ClassroomId = classroomId,
                Date = date
            };

            var result = new List<ClassroomAvailabilityDto> { availability };
            return Response<List<ClassroomAvailabilityDto>>.Success(result, 200);
        }

        public async Task<Response<ClassroomReservationDto>> CreateReservationAsync(string userId, ClassroomReservationCreateDto dto)
        {
            // Geçmiş tarih kontrolü
            if (dto.ReservationDate.Date < DateTime.UtcNow.Date)
                return Response<ClassroomReservationDto>.Fail("Geçmiş tarih için rezervasyon yapılamaz", 400);

            // Hafta sonu kontrolü
            if (dto.ReservationDate.DayOfWeek == DayOfWeek.Saturday || dto.ReservationDate.DayOfWeek == DayOfWeek.Sunday)
                return Response<ClassroomReservationDto>.Fail("Hafta sonu için rezervasyon yapılamaz", 400);

            // Sınıf kontrolü
            var classroom = await _context.Classrooms.FindAsync(dto.ClassroomId);
            if (classroom == null)
                return Response<ClassroomReservationDto>.Fail("Sınıf bulunamadı", 404);

            // Çakışma kontrolü - Ders programı
            var dayOfWeek = dto.ReservationDate.DayOfWeek;
            var scheduleConflict = await _context.Schedules
                .AnyAsync(s => s.ClassroomId == dto.ClassroomId &&
                              s.DayOfWeek == dayOfWeek &&
                              s.IsActive &&
                              (s.StartTime < dto.EndTime && s.EndTime > dto.StartTime));

            if (scheduleConflict)
                return Response<ClassroomReservationDto>.Fail("Bu saat diliminde ders programı mevcut", 400);

            // Çakışma kontrolü - Mevcut onaylı rezervasyonlar
            var reservationConflict = await _context.ClassroomReservations
                .AnyAsync(r => r.ClassroomId == dto.ClassroomId &&
                              r.ReservationDate.Date == dto.ReservationDate.Date &&
                              r.Status == ReservationStatus.Approved &&
                              r.IsActive &&
                              (r.StartTime < dto.EndTime && r.EndTime > dto.StartTime));

            if (reservationConflict)
                return Response<ClassroomReservationDto>.Fail("Bu saat diliminde onaylı rezervasyon mevcut", 400);

            var reservation = new ClassroomReservation
            {
                ClassroomId = dto.ClassroomId,
                RequestedByUserId = userId,
                ReservationDate = dto.ReservationDate.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Purpose = dto.Purpose,
                Status = ReservationStatus.Pending,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _context.ClassroomReservations.AddAsync(reservation);
            await _context.SaveChangesAsync();

            var result = await _context.ClassroomReservations
                .Include(r => r.Classroom)
                .Include(r => r.RequestedBy)
                .FirstAsync(r => r.Id == reservation.Id);

            return Response<ClassroomReservationDto>.Success(MapToDto(result), 201);
        }

        public async Task<Response<NoDataDto>> CancelReservationAsync(string userId, int reservationId)
        {
            var reservation = await _context.ClassroomReservations
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.RequestedByUserId == userId);

            if (reservation == null)
                return Response<NoDataDto>.Fail("Rezervasyon bulunamadı", 404);

            if (reservation.Status == ReservationStatus.Rejected)
                return Response<NoDataDto>.Fail("Reddedilmiş rezervasyon iptal edilemez", 400);

            reservation.Status = ReservationStatus.Cancelled;
            reservation.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> ApproveReservationAsync(string adminUserId, int reservationId, string? notes)
        {
            var reservation = await _context.ClassroomReservations.FindAsync(reservationId);
            if (reservation == null)
                return Response<NoDataDto>.Fail("Rezervasyon bulunamadı", 404);

            if (reservation.Status != ReservationStatus.Pending)
                return Response<NoDataDto>.Fail("Sadece bekleyen rezervasyonlar onaylanabilir", 400);

            // Son çakışma kontrolü
            var reservationConflict = await _context.ClassroomReservations
                .AnyAsync(r => r.Id != reservationId &&
                              r.ClassroomId == reservation.ClassroomId &&
                              r.ReservationDate.Date == reservation.ReservationDate.Date &&
                              r.Status == ReservationStatus.Approved &&
                              r.IsActive &&
                              (r.StartTime < reservation.EndTime && r.EndTime > reservation.StartTime));

            if (reservationConflict)
                return Response<NoDataDto>.Fail("Bu saat diliminde başka bir onaylı rezervasyon oluşturulmuş", 400);

            reservation.Status = ReservationStatus.Approved;
            reservation.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> RejectReservationAsync(string adminUserId, int reservationId, string reason)
        {
            var reservation = await _context.ClassroomReservations.FindAsync(reservationId);
            if (reservation == null)
                return Response<NoDataDto>.Fail("Rezervasyon bulunamadı", 404);

            if (reservation.Status != ReservationStatus.Pending)
                return Response<NoDataDto>.Fail("Sadece bekleyen rezervasyonlar reddedilebilir", 400);

            reservation.Status = ReservationStatus.Rejected;
            reservation.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<List<ClassroomReservationDto>>> GetReservationsByDateAsync(DateTime date, int? classroomId = null)
        {
            var query = _context.ClassroomReservations
                .Include(r => r.Classroom)
                .Include(r => r.RequestedBy)
                .Where(r => r.ReservationDate.Date == date.Date && r.IsActive);

            if (classroomId.HasValue)
                query = query.Where(r => r.ClassroomId == classroomId.Value);

            var reservations = await query
                .OrderBy(r => r.StartTime)
                .Select(r => MapToDto(r))
                .ToListAsync();

            return Response<List<ClassroomReservationDto>>.Success(reservations, 200);
        }

        private static ClassroomReservationDto MapToDto(ClassroomReservation r)
        {
            return new ClassroomReservationDto
            {
                Id = r.Id,
                ClassroomId = r.ClassroomId,
                ClassroomInfo = r.Classroom != null ? $"{r.Classroom.Building} - {r.Classroom.RoomNumber}" : "",
                RequestedByUserId = r.RequestedByUserId,
                RequestedByName = r.RequestedBy?.UserName ?? "",
                StudentLeaderName = r.StudentLeaderName,
                Purpose = r.Purpose,
                ReservationDate = r.ReservationDate,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.Status,
                CreatedAt = r.CreatedDate
            };
        }
    }
}
