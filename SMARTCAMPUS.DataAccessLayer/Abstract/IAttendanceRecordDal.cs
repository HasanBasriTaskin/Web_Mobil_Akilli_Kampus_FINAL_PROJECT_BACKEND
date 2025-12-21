using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IAttendanceRecordDal : IGenericDal<AttendanceRecord>
    {
        Task<IEnumerable<AttendanceRecord>> GetRecordsByStudentAsync(int studentId);
        Task<IEnumerable<AttendanceRecord>> GetRecordsBySessionAsync(int sessionId);
        Task<bool> HasStudentCheckedInAsync(int sessionId, int studentId);
    }
}
