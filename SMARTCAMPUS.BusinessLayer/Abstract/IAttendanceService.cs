using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IAttendanceService
    {
        Task<Response<AttendanceSessionDto>> CreateSessionAsync(AttendanceSessionCreateDto sessionCreateDto);
        Task<Response<NoDataDto>> CheckInAsync(int studentId, int sessionId, AttendanceCheckInDto checkInDto);
        Task<Response<IEnumerable<AttendanceRecordDto>>> GetSessionRecordsAsync(int sessionId);
        Task<Response<IEnumerable<AttendanceRecordDto>>> GetStudentAttendanceAsync(int studentId);
        Task<Response<AttendanceSessionDto>> GetSessionByIdAsync(int sessionId);
        Task<Response<NoDataDto>> CloseSessionAsync(int sessionId, string instructorId);
        Task<Response<IEnumerable<AttendanceSessionDto>>> GetMySessionsAsync(string instructorId);
        Task<Response<AttendanceReportDto>> GetSectionAttendanceReportAsync(int sectionId);
        Task<Response<QrCodeRefreshDto>> RefreshQrCodeAsync(int sessionId);
        Task<Response<MyAttendanceDto>> GetMyAttendanceAsync(int studentId);
    }
}



