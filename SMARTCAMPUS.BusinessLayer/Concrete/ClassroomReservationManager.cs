using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class ClassroomReservationManager : IClassroomReservationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClassroomReservationManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<List<ClassroomReservationDto>>> GetMyReservationsAsync(string userId)
        {
            var reservations = await _unitOfWork.ClassroomReservations.GetByUserIdAsync(userId);
            var result = reservations.Select(r => MapToDto(r)).ToList();
            return Response<List<ClassroomReservationDto>>.Success(result, 200);
        }

        public async Task<Response<List<ClassroomReservationDto>>> GetPendingReservationsAsync()
        {
            var reservations = await _unitOfWork.ClassroomReservations.GetPendingReservationsAsync();
            var result = reservations.Select(r => MapToDto(r)).ToList();
            return Response<List<ClassroomReservationDto>>.Success(result, 200);
        }

        public async Task<Response<List<ClassroomAvailabilityDto>>> GetClassroomAvailabilityAsync(int classroomId, DateTime date)
        {
            var classroom = await _unitOfWork.Classrooms.GetByIdAsync(classroomId);
            if (classroom == null)
                return Response<List<ClassroomAvailabilityDto>>.Fail("Sınıf bulunamadı", 404);

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
            if (dto.ReservationDate.Date < DateTime.UtcNow.Date)
                return Response<ClassroomReservationDto>.Fail("Geçmiş tarih için rezervasyon yapılamaz", 400);

            if (dto.ReservationDate.DayOfWeek == DayOfWeek.Saturday || dto.ReservationDate.DayOfWeek == DayOfWeek.Sunday)
                return Response<ClassroomReservationDto>.Fail("Hafta sonu için rezervasyon yapılamaz", 400);

            var classroom = await _unitOfWork.Classrooms.GetByIdAsync(dto.ClassroomId);
            if (classroom == null)
                return Response<ClassroomReservationDto>.Fail("Sınıf bulunamadı", 404);

            var dayOfWeek = dto.ReservationDate.DayOfWeek;
            var scheduleConflict = await _unitOfWork.Schedules.GetConflictingScheduleAsync(
                dto.ClassroomId, dayOfWeek, dto.StartTime, dto.EndTime);

            if (scheduleConflict != null)
                return Response<ClassroomReservationDto>.Fail("Bu saat diliminde ders programı mevcut", 400);

            var reservationConflict = await _unitOfWork.ClassroomReservations.HasConflictAsync(
                dto.ClassroomId, dto.ReservationDate, dto.StartTime, dto.EndTime);

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

            await _unitOfWork.ClassroomReservations.AddAsync(reservation);
            await _unitOfWork.CommitAsync();

            var result = await _unitOfWork.ClassroomReservations.GetByIdWithDetailsAsync(reservation.Id);
            return Response<ClassroomReservationDto>.Success(MapToDto(result!), 201);
        }

        public async Task<Response<NoDataDto>> CancelReservationAsync(string userId, int reservationId)
        {
            var reservation = await _unitOfWork.ClassroomReservations.GetByIdAsync(reservationId);
            if (reservation == null || reservation.RequestedByUserId != userId)
                return Response<NoDataDto>.Fail("Rezervasyon bulunamadı", 404);

            if (reservation.Status == ReservationStatus.Rejected)
                return Response<NoDataDto>.Fail("Reddedilmiş rezervasyon iptal edilemez", 400);

            reservation.Status = ReservationStatus.Cancelled;
            reservation.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.ClassroomReservations.Update(reservation);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> ApproveReservationAsync(string adminUserId, int reservationId, string? notes)
        {
            var reservation = await _unitOfWork.ClassroomReservations.GetByIdAsync(reservationId);
            if (reservation == null)
                return Response<NoDataDto>.Fail("Rezervasyon bulunamadı", 404);

            if (reservation.Status != ReservationStatus.Pending)
                return Response<NoDataDto>.Fail("Sadece bekleyen rezervasyonlar onaylanabilir", 400);

            var reservationConflict = await _unitOfWork.ClassroomReservations.HasConflictAsync(
                reservation.ClassroomId, reservation.ReservationDate, reservation.StartTime, reservation.EndTime, reservationId);

            if (reservationConflict)
                return Response<NoDataDto>.Fail("Bu saat diliminde başka bir onaylı rezervasyon oluşturulmuş", 400);

            reservation.Status = ReservationStatus.Approved;
            reservation.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.ClassroomReservations.Update(reservation);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> RejectReservationAsync(string adminUserId, int reservationId, string reason)
        {
            var reservation = await _unitOfWork.ClassroomReservations.GetByIdAsync(reservationId);
            if (reservation == null)
                return Response<NoDataDto>.Fail("Rezervasyon bulunamadı", 404);

            if (reservation.Status != ReservationStatus.Pending)
                return Response<NoDataDto>.Fail("Sadece bekleyen rezervasyonlar reddedilebilir", 400);

            reservation.Status = ReservationStatus.Rejected;
            reservation.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.ClassroomReservations.Update(reservation);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<List<ClassroomReservationDto>>> GetReservationsByDateAsync(DateTime date, int? classroomId = null)
        {
            var reservations = await _unitOfWork.ClassroomReservations.GetByDateAsync(date, classroomId);
            var result = reservations.Select(r => MapToDto(r)).ToList();
            return Response<List<ClassroomReservationDto>>.Success(result, 200);
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
