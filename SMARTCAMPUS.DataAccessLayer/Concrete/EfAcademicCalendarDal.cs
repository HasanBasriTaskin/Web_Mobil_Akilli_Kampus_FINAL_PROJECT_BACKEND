using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfAcademicCalendarDal : GenericRepository<AcademicCalendar>, IAcademicCalendarDal
    {
        public EfAcademicCalendarDal(CampusContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AcademicCalendar>> GetCalendarsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AcademicCalendars
                .Where(c => c.IsActive &&
                           ((c.StartDate >= startDate && c.StartDate <= endDate) ||
                            (c.EndDate >= startDate && c.EndDate <= endDate) ||
                            (c.StartDate <= startDate && c.EndDate >= endDate)))
                .OrderBy(c => c.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<AcademicCalendar>> GetCalendarsByYearAndSemesterAsync(int? year, string? semester)
        {
            var query = _context.AcademicCalendars.Where(c => c.IsActive);

            if (year.HasValue)
            {
                query = query.Where(c => c.Year == year.Value);
            }

            if (!string.IsNullOrEmpty(semester))
            {
                query = query.Where(c => c.Semester == semester);
            }

            return await query.OrderBy(c => c.StartDate).ToListAsync();
        }

        public async Task<IEnumerable<AcademicCalendar>> GetUpcomingEventsAsync(int days = 30)
        {
            var today = DateTime.UtcNow.Date;
            var futureDate = today.AddDays(days);

            return await _context.AcademicCalendars
                .Where(c => c.IsActive &&
                           c.StartDate >= today &&
                           c.StartDate <= futureDate)
                .OrderBy(c => c.StartDate)
                .ToListAsync();
        }
    }
}
