using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IAcademicCalendarService
    {
        Task<Response<IEnumerable<AcademicCalendarDto>>> GetCalendarsAsync(int? year = null, string? semester = null);
        Task<Response<IEnumerable<AcademicCalendarDto>>> GetCalendarsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Response<IEnumerable<AcademicCalendarDto>>> GetUpcomingEventsAsync(int days = 30);
    }
}
