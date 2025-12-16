using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Attendance;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IAttendanceService
    {
        // Session management (Faculty)
        Task<Response<AttendanceSessionDto>> CreateSessionAsync(int instructorId, CreateSessionDto dto);
        Task<Response<AttendanceSessionDto>> GetSessionByIdAsync(int sessionId);
        Task<Response<NoDataDto>> CloseSessionAsync(int instructorId, int sessionId);
        Task<Response<IEnumerable<AttendanceSessionDto>>> GetMySessionsAsync(int instructorId);
        Task<Response<IEnumerable<AttendanceRecordDto>>> GetSessionRecordsAsync(int sessionId);
        
        // Check-in (Student)
        Task<Response<CheckInResultDto>> CheckInAsync(int studentId, int sessionId, CheckInDto dto);
        Task<Response<IEnumerable<StudentAttendanceDto>>> GetMyAttendanceAsync(int studentId);
        
        // Excuse requests
        Task<Response<ExcuseRequestDto>> CreateExcuseRequestAsync(int studentId, CreateExcuseRequestDto dto, string? documentUrl);
        Task<Response<IEnumerable<ExcuseRequestDto>>> GetExcuseRequestsAsync(int instructorId, int? sectionId = null);
        Task<Response<NoDataDto>> ApproveExcuseRequestAsync(int instructorId, int requestId, ReviewExcuseRequestDto dto);
        Task<Response<NoDataDto>> RejectExcuseRequestAsync(int instructorId, int requestId, ReviewExcuseRequestDto dto);
        
        // Utility
        double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
    }
}
