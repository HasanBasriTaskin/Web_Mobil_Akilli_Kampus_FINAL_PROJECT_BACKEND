using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IAttendanceSessionDal : IGenericDal<AttendanceSession>
    {
        Task<AttendanceSession?> GetSessionWithRecordsAsync(int id);
        Task<IEnumerable<AttendanceSession>> GetSessionsBySectionAsync(int sectionId);
        Task<IEnumerable<AttendanceSession>> GetSessionsByInstructorAsync(int instructorId);
    }
}
