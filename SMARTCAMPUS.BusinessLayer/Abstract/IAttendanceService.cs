using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IAttendanceService
    {
        Task<Response<AttendanceSessionDto>> CreateSessionAsync(AttendanceSessionCreateDto sessionCreateDto);
        Task<Response<NoDataDto>> CheckInAsync(int studentId, int sessionId, AttendanceCheckInDto checkInDto);
        Task<Response<IEnumerable<AttendanceRecordDto>>> GetSessionRecordsAsync(int sessionId, string? instructorId = null, bool isAdmin = false);
        Task<Response<IEnumerable<AttendanceRecordDto>>> GetStudentAttendanceAsync(int studentId, int? requestingStudentId = null, bool isAdmin = false, string? instructorId = null);
        Task<Response<AttendanceSessionDto>> GetSessionByIdAsync(int sessionId, int? studentId = null, string? instructorId = null, bool isAdmin = false);
        Task<Response<NoDataDto>> CloseSessionAsync(int sessionId, string instructorId);
        Task<Response<IEnumerable<AttendanceSessionDto>>> GetMySessionsAsync(string instructorId);
        Task<Response<AttendanceReportDto>> GetSectionAttendanceReportAsync(int sectionId, string? instructorId = null, bool isAdmin = false);
        Task<Response<QrCodeRefreshDto>> RefreshQrCodeAsync(int sessionId, string? instructorId = null, bool isAdmin = false);
        Task<Response<MyAttendanceDto>> GetMyAttendanceAsync(int studentId);
    }
}



