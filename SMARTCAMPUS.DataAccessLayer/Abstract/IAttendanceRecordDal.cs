using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IAttendanceRecordDal : IGenericDal<AttendanceRecord>
    {
        Task<AttendanceRecord?> GetRecordBySessionAndStudentAsync(int sessionId, int studentId);
        Task<IEnumerable<AttendanceRecord>> GetRecordsBySessionAsync(int sessionId);
        Task<IEnumerable<AttendanceRecord>> GetRecordsByStudentAsync(int studentId);
        Task<IEnumerable<AttendanceRecord>> GetRecordsBySectionAsync(int sectionId);
    }
}



