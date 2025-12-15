using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Attendance;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class AttendanceManager : IAttendanceService
    {
        private readonly IAttendanceSessionDal _sessionDal;
        private readonly IAttendanceRecordDal _recordDal;
        private readonly IExcuseRequestDal _excuseDal;
        private readonly CampusContext _context;

        public AttendanceManager(
            IAttendanceSessionDal sessionDal,
            IAttendanceRecordDal recordDal,
            IExcuseRequestDal excuseDal,
            CampusContext context)
        {
            _sessionDal = sessionDal;
            _recordDal = recordDal;
            _excuseDal = excuseDal;
            _context = context;
        }

        public async Task<Response<AttendanceSessionDto>> CreateSessionAsync(int instructorId, CreateSessionDto dto)
        {
            // Verify instructor owns this section
            var section = await _context.CourseSections
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == dto.SectionId && s.InstructorId == instructorId);

            if (section == null)
                return Response<AttendanceSessionDto>.Fail("Section not found or access denied", 404);

            // Generate QR Code (simple unique identifier)
            var qrCode = $"ATTEND-{dto.SectionId}-{DateTime.UtcNow.Ticks}-{Guid.NewGuid():N}";

            var session = new AttendanceSession
            {
                SectionId = dto.SectionId,
                InstructorId = instructorId,
                Date = dto.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                GeofenceRadius = dto.GeofenceRadius,
                QRCode = qrCode,
                Status = AttendanceSessionStatus.Open
            };

            await _sessionDal.AddAsync(session);
            await _context.SaveChangesAsync();

            var enrolledCount = await _context.Enrollments
                .CountAsync(e => e.SectionId == dto.SectionId && e.Status == EnrollmentStatus.Enrolled);

            var result = new AttendanceSessionDto
            {
                Id = session.Id,
                Date = session.Date,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Latitude = session.Latitude,
                Longitude = session.Longitude,
                GeofenceRadius = session.GeofenceRadius,
                QRCode = qrCode,
                Status = session.Status,
                SectionId = dto.SectionId,
                CourseCode = section.Course.Code,
                CourseName = section.Course.Name,
                SectionNumber = section.SectionNumber,
                TotalStudents = enrolledCount,
                PresentCount = 0,
                AbsentCount = enrolledCount
            };

            return Response<AttendanceSessionDto>.Success(result, 201);
        }

        public async Task<Response<AttendanceSessionDto>> GetSessionByIdAsync(int sessionId)
        {
            var session = await _sessionDal.GetSessionWithRecordsAsync(sessionId);
            if (session == null)
                return Response<AttendanceSessionDto>.Fail("Session not found", 404);

            var enrolledCount = await _context.Enrollments
                .CountAsync(e => e.SectionId == session.SectionId && e.Status == EnrollmentStatus.Enrolled);

            var result = new AttendanceSessionDto
            {
                Id = session.Id,
                Date = session.Date,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Latitude = session.Latitude,
                Longitude = session.Longitude,
                GeofenceRadius = session.GeofenceRadius,
                QRCode = session.QRCode,
                Status = session.Status,
                SectionId = session.SectionId,
                CourseCode = session.Section.Course.Code,
                CourseName = session.Section.Course.Name,
                SectionNumber = session.Section.SectionNumber,
                TotalStudents = enrolledCount,
                PresentCount = session.AttendanceRecords.Count,
                AbsentCount = enrolledCount - session.AttendanceRecords.Count
            };

            return Response<AttendanceSessionDto>.Success(result, 200);
        }

        public async Task<Response<NoDataDto>> CloseSessionAsync(int instructorId, int sessionId)
        {
            var session = await _context.AttendanceSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.InstructorId == instructorId);

            if (session == null)
                return Response<NoDataDto>.Fail("Session not found or access denied", 404);

            if (session.Status != AttendanceSessionStatus.Open)
                return Response<NoDataDto>.Fail("Session is not open", 400);

            session.Status = AttendanceSessionStatus.Closed;
            _sessionDal.Update(session);
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<IEnumerable<AttendanceSessionDto>>> GetMySessionsAsync(int instructorId)
        {
            var sessions = await _sessionDal.GetSessionsByInstructorAsync(instructorId);

            var result = sessions.Select(s => new AttendanceSessionDto
            {
                Id = s.Id,
                Date = s.Date,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Status = s.Status,
                SectionId = s.SectionId,
                CourseCode = s.Section.Course.Code,
                CourseName = s.Section.Course.Name,
                SectionNumber = s.Section.SectionNumber
            });

            return Response<IEnumerable<AttendanceSessionDto>>.Success(result, 200);
        }

        public async Task<Response<IEnumerable<AttendanceRecordDto>>> GetSessionRecordsAsync(int sessionId)
        {
            var records = await _recordDal.GetRecordsBySessionAsync(sessionId);

            var result = records.Select(r => new AttendanceRecordDto
            {
                Id = r.Id,
                CheckInTime = r.CheckInTime,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                DistanceFromCenter = r.DistanceFromCenter,
                IsFlagged = r.IsFlagged,
                FlagReason = r.FlagReason,
                StudentId = r.StudentId,
                StudentNumber = r.Student.StudentNumber,
                StudentName = r.Student.User.FullName
            });

            return Response<IEnumerable<AttendanceRecordDto>>.Success(result, 200);
        }

        public async Task<Response<CheckInResultDto>> CheckInAsync(int studentId, int sessionId, CheckInDto dto)
        {
            var session = await _context.AttendanceSessions.FindAsync(sessionId);
            if (session == null)
                return Response<CheckInResultDto>.Fail("Session not found", 404);

            if (session.Status != AttendanceSessionStatus.Open)
                return Response<CheckInResultDto>.Fail("Session is not open", 400);

            // Check if student is enrolled in this section
            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.SectionId == session.SectionId && e.Status == EnrollmentStatus.Enrolled);

            if (!isEnrolled)
                return Response<CheckInResultDto>.Fail("Not enrolled in this course", 403);

            // Check if already checked in
            var alreadyCheckedIn = await _recordDal.HasStudentCheckedInAsync(sessionId, studentId);
            if (alreadyCheckedIn)
                return Response<CheckInResultDto>.Fail("Already checked in", 400);

            // Calculate distance using Haversine formula
            var distance = CalculateDistance(session.Latitude, session.Longitude, dto.Latitude, dto.Longitude);

            // Detect spoofing (check for suspicious patterns)
            bool isFlagged = false;
            string? flagReason = null;

            if (distance > session.GeofenceRadius)
            {
                isFlagged = true;
                flagReason = $"Outside geofence: {distance:F1}m from center (max: {session.GeofenceRadius}m)";
            }

            // Check for GPS accuracy (if provided)
            if (dto.Accuracy.HasValue && dto.Accuracy.Value > 100)
            {
                isFlagged = true;
                flagReason = $"Low GPS accuracy: {dto.Accuracy.Value}m";
            }

            var record = new AttendanceRecord
            {
                SessionId = sessionId,
                StudentId = studentId,
                CheckInTime = DateTime.UtcNow,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                DistanceFromCenter = distance,
                IsFlagged = isFlagged,
                FlagReason = flagReason
            };

            await _recordDal.AddAsync(record);
            await _context.SaveChangesAsync();

            var result = new CheckInResultDto
            {
                Success = true,
                Message = isFlagged ? "Check-in recorded but flagged for review" : "Check-in successful",
                DistanceFromCenter = distance,
                IsFlagged = isFlagged,
                FlagReason = flagReason
            };

            return Response<CheckInResultDto>.Success(result, 200);
        }

        public async Task<Response<IEnumerable<StudentAttendanceDto>>> GetMyAttendanceAsync(int studentId)
        {
            // Get all enrollments for the student
            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Enrolled)
                .Include(e => e.Section)
                    .ThenInclude(s => s.Course)
                .ToListAsync();

            var result = new List<StudentAttendanceDto>();

            foreach (var enrollment in enrollments)
            {
                var sessions = await _context.AttendanceSessions
                    .Where(s => s.SectionId == enrollment.SectionId)
                    .ToListAsync();

                var attendedCount = await _context.AttendanceRecords
                    .CountAsync(r => r.StudentId == studentId && r.Session.SectionId == enrollment.SectionId);

                var excusedCount = await _context.ExcuseRequests
                    .CountAsync(er => er.StudentId == studentId 
                        && er.Session.SectionId == enrollment.SectionId 
                        && er.Status == ExcuseRequestStatus.Approved);

                var totalSessions = sessions.Count;
                var effectiveAttendance = totalSessions > 0 
                    ? (attendedCount + excusedCount) * 100.0 / totalSessions 
                    : 100;

                var warningLevel = effectiveAttendance switch
                {
                    >= 80 => "OK",
                    >= 70 => "Warning",
                    _ => "Critical"
                };

                result.Add(new StudentAttendanceDto
                {
                    CourseCode = enrollment.Section.Course.Code,
                    CourseName = enrollment.Section.Course.Name,
                    TotalSessions = totalSessions,
                    AttendedSessions = attendedCount,
                    ExcusedSessions = excusedCount,
                    AttendancePercentage = Math.Round(effectiveAttendance, 1),
                    WarningLevel = warningLevel
                });
            }

            return Response<IEnumerable<StudentAttendanceDto>>.Success(result, 200);
        }

        public async Task<Response<ExcuseRequestDto>> CreateExcuseRequestAsync(int studentId, CreateExcuseRequestDto dto, string? documentUrl)
        {
            var session = await _context.AttendanceSessions
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .FirstOrDefaultAsync(s => s.Id == dto.SessionId);

            if (session == null)
                return Response<ExcuseRequestDto>.Fail("Session not found", 404);

            var excuseRequest = new ExcuseRequest
            {
                StudentId = studentId,
                SessionId = dto.SessionId,
                Reason = dto.Reason,
                DocumentUrl = documentUrl,
                Status = ExcuseRequestStatus.Pending
            };

            await _excuseDal.AddAsync(excuseRequest);
            await _context.SaveChangesAsync();

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            var result = new ExcuseRequestDto
            {
                Id = excuseRequest.Id,
                Reason = excuseRequest.Reason,
                DocumentUrl = excuseRequest.DocumentUrl,
                Status = excuseRequest.Status,
                CreatedDate = excuseRequest.CreatedDate,
                StudentId = studentId,
                StudentNumber = student?.StudentNumber ?? "",
                StudentName = student?.User.FullName ?? "",
                SessionId = dto.SessionId,
                SessionDate = session.Date,
                CourseCode = session.Section.Course.Code,
                CourseName = session.Section.Course.Name
            };

            return Response<ExcuseRequestDto>.Success(result, 201);
        }

        public async Task<Response<IEnumerable<ExcuseRequestDto>>> GetExcuseRequestsAsync(int instructorId, int? sectionId = null)
        {
            var query = _context.ExcuseRequests
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .Include(r => r.Session)
                    .ThenInclude(s => s.Section)
                        .ThenInclude(sec => sec.Course)
                .Where(r => r.Session.InstructorId == instructorId);

            if (sectionId.HasValue)
                query = query.Where(r => r.Session.SectionId == sectionId.Value);

            var requests = await query.OrderByDescending(r => r.CreatedDate).ToListAsync();

            var result = requests.Select(r => new ExcuseRequestDto
            {
                Id = r.Id,
                Reason = r.Reason,
                DocumentUrl = r.DocumentUrl,
                Status = r.Status,
                CreatedDate = r.CreatedDate,
                ReviewedAt = r.ReviewedAt,
                Notes = r.Notes,
                StudentId = r.StudentId,
                StudentNumber = r.Student.StudentNumber,
                StudentName = r.Student.User.FullName,
                SessionId = r.SessionId,
                SessionDate = r.Session.Date,
                CourseCode = r.Session.Section.Course.Code,
                CourseName = r.Session.Section.Course.Name
            });

            return Response<IEnumerable<ExcuseRequestDto>>.Success(result, 200);
        }

        public async Task<Response<NoDataDto>> ApproveExcuseRequestAsync(int instructorId, int requestId, ReviewExcuseRequestDto dto)
        {
            var request = await _context.ExcuseRequests
                .Include(r => r.Session)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Session.InstructorId == instructorId);

            if (request == null)
                return Response<NoDataDto>.Fail("Request not found or access denied", 404);

            request.Status = ExcuseRequestStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedById = instructorId.ToString();
            request.Notes = dto.Notes;

            _excuseDal.Update(request);
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> RejectExcuseRequestAsync(int instructorId, int requestId, ReviewExcuseRequestDto dto)
        {
            var request = await _context.ExcuseRequests
                .Include(r => r.Session)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Session.InstructorId == instructorId);

            if (request == null)
                return Response<NoDataDto>.Fail("Request not found or access denied", 404);

            request.Status = ExcuseRequestStatus.Rejected;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedById = instructorId.ToString();
            request.Notes = dto.Notes;

            _excuseDal.Update(request);
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        /// <summary>
        /// Calculates distance between two GPS coordinates using Haversine formula
        /// </summary>
        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Earth's radius in meters

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // Distance in meters
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180;
    }
}
