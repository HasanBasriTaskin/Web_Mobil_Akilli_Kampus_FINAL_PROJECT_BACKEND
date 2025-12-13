using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IAttendanceService
    {
        Task<Response<AttendanceSessionDto>> CreateSessionAsync(AttendanceSessionDto sessionDto);
        Task<Response<NoDataDto>> CheckInAsync(int studentId, AttendanceCheckInDto checkInDto);
        Task<Response<IEnumerable<AttendanceRecordDto>>> GetSessionRecordsAsync(int sessionId);
        Task<Response<IEnumerable<AttendanceRecordDto>>> GetStudentAttendanceAsync(int studentId);
        Task<Response<AttendanceSessionDto>> GetSessionByIdAsync(int sessionId);
    }
}



