using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IAttendanceSessionDal : IGenericDal<AttendanceSession>
    {
        Task<AttendanceSession?> GetSessionWithRecordsAsync(int sessionId);
        Task<IEnumerable<AttendanceSession>> GetSessionsBySectionAsync(int sectionId);
        Task<IEnumerable<AttendanceSession>> GetSessionsByInstructorAsync(string instructorId);
        Task<IEnumerable<AttendanceSession>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}

