using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfClassroomReservationDal : GenericRepository<ClassroomReservation>, IClassroomReservationDal
    {
        public EfClassroomReservationDal(CampusContext context) : base(context)
        {
        }

        public async Task<ClassroomReservation?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.ClassroomReservations
                .Include(r => r.Classroom)
                .Include(r => r.RequestedBy)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<ClassroomReservation>> GetByUserIdAsync(string userId)
        {
            return await _context.ClassroomReservations
                .Include(r => r.Classroom)
                .Where(r => r.RequestedByUserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
        }

        public async Task<List<ClassroomReservation>> GetByDateAsync(DateTime date, int? classroomId = null)
        {
            var query = _context.ClassroomReservations
                .Include(r => r.Classroom)
                .Include(r => r.RequestedBy)
                .Where(r => r.ReservationDate.Date == date.Date);

            if (classroomId.HasValue)
                query = query.Where(r => r.ClassroomId == classroomId.Value);

            return await query.OrderBy(r => r.StartTime).ToListAsync();
        }

        public async Task<List<ClassroomReservation>> GetPendingAsync()
        {
            return await _context.ClassroomReservations
                .Include(r => r.Classroom)
                .Include(r => r.RequestedBy)
                .Where(r => r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<ClassroomReservation>> GetPendingReservationsAsync()
        {
            return await GetPendingAsync();
        }

        public async Task<List<ClassroomReservation>> GetConflictsAsync(int classroomId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            var query = _context.ClassroomReservations
                .Where(r => r.ClassroomId == classroomId &&
                            r.ReservationDate.Date == date.Date &&
                            r.Status == ReservationStatus.Approved &&
                            ((r.StartTime < endTime && r.EndTime > startTime)));

            if (excludeId.HasValue)
                query = query.Where(r => r.Id != excludeId.Value);

            return await query.ToListAsync();
        }

        public async Task<bool> HasConflictAsync(int classroomId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            var query = _context.ClassroomReservations
                .Where(r => r.ClassroomId == classroomId &&
                            r.ReservationDate.Date == date.Date &&
                            r.Status == ReservationStatus.Approved &&
                            ((r.StartTime < endTime && r.EndTime > startTime)));

            if (excludeId.HasValue)
                query = query.Where(r => r.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}
