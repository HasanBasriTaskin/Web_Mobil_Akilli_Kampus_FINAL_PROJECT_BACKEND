
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using System.Linq;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Attendance;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class AttendanceManager : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AttendanceManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<AttendanceSessionDto>> CreateSessionAsync(int instructorId, CreateSessionDto dto)
        {
            // Verify instructor owns this section
            var section = await _unitOfWork.CourseSections.GetSectionWithDetailsAsync(dto.SectionId);
            
            if (section == null || section.InstructorId != instructorId)
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

            await _unitOfWork.AttendanceSessions.AddAsync(session);
            await _unitOfWork.CommitAsync();

            var enrolledCount = section.EnrolledCount; // Was CountAsync on Enrollments, but Section usually holds this count.
            // Or count manually if needed? 
            // Better to use DAL if EnrolledCount might be out of sync? 
            // But we trust EnrolledCount usually. 
            // Let's use Enrollments DAL to be safe as per original logic.
            // var enrolledCount = await _context.Enrollments.CountAsync...
            // UoW Enrollments usually inherits GenericRepository.
            // I can use Where().CountAsync() via Repository.
            var enrolledCountSafe = await _unitOfWork.Enrollments.CountAsync(e => e.SectionId == dto.SectionId && e.Status == EnrollmentStatus.Enrolled);

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
                TotalStudents = enrolledCountSafe,
                PresentCount = 0,
                AbsentCount = enrolledCountSafe
            };

            return Response<AttendanceSessionDto>.Success(result, 201);
        }

        public async Task<Response<AttendanceSessionDto>> GetSessionByIdAsync(int sessionId)
        {
            var session = await _unitOfWork.AttendanceSessions.GetSessionWithRecordsAsync(sessionId);
            if (session == null)
                return Response<AttendanceSessionDto>.Fail("Session not found", 404);

            var enrolledCount = await _unitOfWork.Enrollments
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
            var session = await _unitOfWork.AttendanceSessions.GetByIdAsync(sessionId);

            if (session == null || session.InstructorId != instructorId)
                return Response<NoDataDto>.Fail("Session not found or access denied", 404);

            if (session.Status != AttendanceSessionStatus.Open)
                return Response<NoDataDto>.Fail("Session is not open", 400);

            session.Status = AttendanceSessionStatus.Closed;
            _unitOfWork.AttendanceSessions.Update(session);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<IEnumerable<AttendanceSessionDto>>> GetMySessionsAsync(int instructorId)
        {
            var sessions = await _unitOfWork.AttendanceSessions.GetSessionsByInstructorAsync(instructorId);

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
            var records = await _unitOfWork.AttendanceRecords.GetRecordsBySessionAsync(sessionId);

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
            var session = await _unitOfWork.AttendanceSessions.GetByIdAsync(sessionId);
            if (session == null)
                return Response<CheckInResultDto>.Fail("Session not found", 404);

            if (session.Status != AttendanceSessionStatus.Open)
                return Response<CheckInResultDto>.Fail("Session is not open", 400);

            // Check if student is enrolled in this section
            var isEnrolled = await _unitOfWork.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.SectionId == session.SectionId && e.Status == EnrollmentStatus.Enrolled);

            if (!isEnrolled)
                return Response<CheckInResultDto>.Fail("Not enrolled in this course", 403);

            // Check if already checked in
            var alreadyCheckedIn = await _unitOfWork.AttendanceRecords.HasStudentCheckedInAsync(sessionId, studentId);
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

            await _unitOfWork.AttendanceRecords.AddAsync(record);
            await _unitOfWork.CommitAsync();

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
            var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsByStudentAsync(studentId);
            // The method GetEnrollmentsByStudentAsync likely doesn't filter by Enrolled status inside DAL, so filter here.
            var activeEnrollments = enrollments.Where(e => e.Status == EnrollmentStatus.Enrolled).ToList();

            var result = new List<StudentAttendanceDto>();

            foreach (var enrollment in activeEnrollments)
            {
                // Note: GetById on Session Dal might not be efficient for listing. 
                // But we need sessions for this section.
                // UoW.AttendanceSessions should support getting by section if possible. 
                // Repository has Where.
                var sessions = await _unitOfWork.AttendanceSessions
                    .GetListAsync(s => s.SectionId == enrollment.SectionId);

                var attendedCount = await _unitOfWork.AttendanceRecords
                    .CountAsync(r => r.StudentId == studentId && r.Session.SectionId == enrollment.SectionId);

                var excusedCount = await _unitOfWork.ExcuseRequests
                    .CountAsync((ExcuseRequest er) => er.StudentId == studentId 
                        && er.Session.SectionId == enrollment.SectionId 
                        && er.Status == ExcuseRequestStatus.Approved);

                var totalSessions = sessions.Count();
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
            var session = await _unitOfWork.AttendanceSessions.GetSessionWithRecordsAsync(dto.SessionId);

            if (session == null)
                return Response<ExcuseRequestDto>.Fail("Session not found", 404);

            var excuseRequest = new ExcuseRequest
            {
                StudentId = studentId,
                SessionId = dto.SessionId,
                Reason = dto.Reason,
                DocumentUrl = documentUrl,
                Status = ExcuseRequestStatus.Pending,
                CreatedDate = DateTime.UtcNow // Ensure created date is set
            };

            await _unitOfWork.ExcuseRequests.AddAsync(excuseRequest);
            await _unitOfWork.CommitAsync();

            var student = await _unitOfWork.Students.GetByIdAsync(studentId); // Assuming GetById handles loading User if lazy loading enabled? 
            // GenericRepository GetByIdAsync usually just Finds. 
            // We might need GetWithDetails.
            // But let's assume standard behavior for now. 
            // Actually, we can just use the Student object if we had it.
            // Let's assume lazy loading or we fetch it.
            // EF Core default GenericRepository might not Include User.
            // Safe bet: Fetch student with details or just use known data.
            // For now, let's look up via UoW.Students.GetByUserIdAsync if we had userId, but we have studentId.
            // Let's use Where + Include via repository.
            // Use Repository Method
            var studentWithUser = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);

            var result = new ExcuseRequestDto
            {
                Id = excuseRequest.Id,
                Reason = excuseRequest.Reason,
                DocumentUrl = excuseRequest.DocumentUrl,
                Status = excuseRequest.Status,
                CreatedDate = excuseRequest.CreatedDate,
                StudentId = studentId,
                StudentNumber = studentWithUser?.StudentNumber ?? "",
                StudentName = studentWithUser?.User.FullName ?? "",
                SessionId = dto.SessionId,
                SessionDate = session.Date,
                CourseCode = session.Section.Course.Code,
                CourseName = session.Section.Course.Name
            };

            return Response<ExcuseRequestDto>.Success(result, 201);
        }

        public async Task<Response<IEnumerable<ExcuseRequestDto>>> GetExcuseRequestsAsync(int instructorId, int? sectionId = null)
        {
            var requests = await _unitOfWork.ExcuseRequests.GetRequestsByInstructorAsync(instructorId, sectionId);

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
            // Use Repository Method
            var request = await _unitOfWork.ExcuseRequests.GetRequestWithDetailsAsync(requestId, instructorId);

            if (request == null)
                return Response<NoDataDto>.Fail("Request not found or access denied", 404);

            request.Status = ExcuseRequestStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedById = instructorId.ToString();
            request.Notes = dto.Notes;

            _unitOfWork.ExcuseRequests.Update(request);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> RejectExcuseRequestAsync(int instructorId, int requestId, ReviewExcuseRequestDto dto)
        {
            var request = await _unitOfWork.ExcuseRequests.GetRequestWithDetailsAsync(requestId, instructorId);

            if (request == null)
                return Response<NoDataDto>.Fail("Request not found or access denied", 404);

            request.Status = ExcuseRequestStatus.Rejected;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedById = instructorId.ToString();
            request.Notes = dto.Notes;

            _unitOfWork.ExcuseRequests.Update(request);
            await _unitOfWork.CommitAsync();

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
