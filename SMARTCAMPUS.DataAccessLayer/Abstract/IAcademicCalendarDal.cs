using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IAcademicCalendarDal : IGenericDal<AcademicCalendar>
    {
        Task<IEnumerable<AcademicCalendar>> GetCalendarsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<AcademicCalendar>> GetCalendarsByYearAndSemesterAsync(int? year, string? semester);
        Task<IEnumerable<AcademicCalendar>> GetUpcomingEventsAsync(int days = 30);
    }
}
