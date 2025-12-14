using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.BusinessLayer.Constants;
using SMARTCAMPUS.BusinessLayer.Tools;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Constants;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using SMARTCAMPUS.EntityLayer.Models;
using System.Text.Json;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly CampusContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const decimal DefaultGeofenceRadius = 15; // meters
        private const int QrCodeExpiryMinutes = 30;
        private const int QrCodeRefreshSeconds = 5; // QR code refreshes every 5 seconds

        public AttendanceService(IUnitOfWork unitOfWork, IMapper mapper, CampusContext context, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response<AttendanceSessionDto>> CreateSessionAsync(AttendanceSessionCreateDto sessionCreateDto)
        {
            try
            {
                // Verify section exists
                var section = await _unitOfWork.CourseSections.GetSectionWithDetailsAsync(sessionCreateDto.SectionId);
                if (section == null)
                    return Response<AttendanceSessionDto>.Fail("Section not found", 404);

                // Get classroom GPS coordinates if not provided
                decimal? latitude = sessionCreateDto.Latitude;
                decimal? longitude = sessionCreateDto.Longitude;

                if (!latitude.HasValue || !longitude.HasValue)
                {
                    if (section.ClassroomId.HasValue)
                    {
                        var classroom = await _unitOfWork.Classrooms.GetByIdAsync(section.ClassroomId.Value);
                        if (classroom != null && !string.IsNullOrEmpty(classroom.FeaturesJson))
                        {
                            try
                            {
                                var features = JsonSerializer.Deserialize<Dictionary<string, object>>(classroom.FeaturesJson);
                                if (features != null && features.ContainsKey("latitude") && features.ContainsKey("longitude"))
                                {
                                    latitude = Convert.ToDecimal(features["latitude"]);
                                    longitude = Convert.ToDecimal(features["longitude"]);
                                }
                            }
                            catch
                            {
                                // If parsing fails, use provided or null values
                            }
                        }
                    }
                }

                // Set default geofence radius if not provided
                var geofenceRadius = sessionCreateDto.GeofenceRadius ?? DefaultGeofenceRadius;

                // Generate unique QR code with expiry
                var qrCode = GenerateQrCode(sessionCreateDto.SectionId, sessionCreateDto.Date);
                var qrCodeGeneratedAt = DateTime.UtcNow;
                var qrCodeExpiresAt = qrCodeGeneratedAt.AddMinutes(QrCodeExpiryMinutes);

                // Get instructor ID from section if not provided in DTO
                var instructorId = section.InstructorId;
                if (string.IsNullOrEmpty(instructorId))
                    return Response<AttendanceSessionDto>.Fail("Section does not have an assigned instructor", 400);

                var session = new AttendanceSession
                {
                    SectionId = sessionCreateDto.SectionId,
                    InstructorId = instructorId,
                    Date = sessionCreateDto.Date,
                    StartTime = sessionCreateDto.StartTime,
                    EndTime = sessionCreateDto.EndTime,
                    Latitude = latitude,
                    Longitude = longitude,
                    GeofenceRadius = geofenceRadius,
                    QrCode = qrCode,
                    QrCodeGeneratedAt = qrCodeGeneratedAt,
                    QrCodeExpiresAt = qrCodeExpiresAt,
                    Status = AttendanceSessionStatus.Scheduled
                };

                await _unitOfWork.AttendanceSessions.AddAsync(session);
                await _unitOfWork.CommitAsync();

                // TODO: Send push notification to enrolled students
                // await _notificationService.SendAttendanceSessionNotificationAsync(session.SectionId, session);

                var resultDto = _mapper.Map<AttendanceSessionDto>(session);
                resultDto.CourseCode = section.Course?.Code;
                resultDto.CourseName = section.Course?.Name;
                resultDto.InstructorName = section.Instructor?.FullName;

                // Calculate statistics
                var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsBySectionAsync(session.SectionId);
                resultDto.TotalStudents = enrollments.Count(e => e.Status == EnrollmentStatus.Active);
                resultDto.PresentCount = 0;
                resultDto.AbsentCount = resultDto.TotalStudents;

                return Response<AttendanceSessionDto>.Success(resultDto, 201);
            }
            catch (Exception ex)
            {
                return Response<AttendanceSessionDto>.Fail($"Error creating attendance session: {ex.Message}", 500);
            }
        }

        public async Task<Response<NoDataDto>> CheckInAsync(int studentId, int sessionId, AttendanceCheckInDto checkInDto)
        {
            try
            {
                // Get session with details
                var session = await _unitOfWork.AttendanceSessions.GetSessionWithRecordsAsync(sessionId);
                if (session == null)
                    return Response<NoDataDto>.Fail("Attendance session not found", 404);

                // Check if session is active
                if (session.Status != SMARTCAMPUS.EntityLayer.Constants.AttendanceSessionStatus.Active && 
                    session.Status != SMARTCAMPUS.EntityLayer.Constants.AttendanceSessionStatus.Scheduled)
                    return Response<NoDataDto>.Fail("Session is not available for check-in", 400);

                // Check if student is enrolled in the section
                var enrollment = await _unitOfWork.Enrollments
                    .GetEnrollmentByStudentAndSectionAsync(studentId, session.SectionId);
                
                if (enrollment == null || enrollment.Status != EnrollmentStatus.Active)
                    return Response<NoDataDto>.Fail("Student is not enrolled in this section", 403);

                // Check if already checked in
                var existingRecord = await _unitOfWork.AttendanceRecords
                    .GetRecordBySessionAndStudentAsync(sessionId, studentId);
                
                if (existingRecord != null)
                    return Response<NoDataDto>.Fail("Already checked in", 400);

                // Verify QR code if provided (with expiry check)
                if (!string.IsNullOrEmpty(checkInDto.QrCode))
                {
                    if (string.IsNullOrEmpty(session.QrCode))
                        return Response<NoDataDto>.Fail("QR code not available for this session", 400);

                    if (checkInDto.QrCode != session.QrCode)
                        return Response<NoDataDto>.Fail("Invalid QR code", 400);

                    // Check QR code expiry
                    if (session.QrCodeExpiresAt.HasValue && DateTime.UtcNow > session.QrCodeExpiresAt.Value)
                        return Response<NoDataDto>.Fail("QR code has expired", 400);
                }

                // Get client IP address
                var clientIpAddress = IpAddressHelper.GetClientIpAddress(_httpContextAccessor.HttpContext);

                // IP Address Validation - Check if student is on campus network
                var isCampusIp = IpAddressHelper.IsCampusIpAddress(clientIpAddress);
                if (!isCampusIp)
                    return Response<NoDataDto>.Fail("You must be connected to the campus network to check in", 403);

                var checkInTime = DateTime.UtcNow;
                var sessionDateTime = session.Date.Add(session.StartTime);
                var isLate = checkInTime > sessionDateTime.AddMinutes(Constants.GradeConstants.LateCheckInGracePeriodMinutes);

                decimal? distanceFromCenter = null;
                bool isFlagged = false;
                string? flagReason = null;
                int fraudScore = 0;

                // Geofencing check
                if (session.Latitude.HasValue && session.Longitude.HasValue &&
                    checkInDto.Latitude.HasValue && checkInDto.Longitude.HasValue)
                {
                    distanceFromCenter = CalculateDistance(
                        session.Latitude.Value,
                        session.Longitude.Value,
                        checkInDto.Latitude.Value,
                        checkInDto.Longitude.Value);

                    // Validate distance <= radius + accuracy buffer (e.g., radius + 5m)
                    var accuracyBuffer = checkInDto.Accuracy ?? 5; // Default 5m buffer
                    var effectiveRadius = session.GeofenceRadius.HasValue 
                        ? session.GeofenceRadius.Value + accuracyBuffer 
                        : DefaultGeofenceRadius + accuracyBuffer;

                    if (distanceFromCenter > effectiveRadius)
                    {
                        isFlagged = true;
                        flagReason = "Outside geofence";
                        fraudScore += 20;
                    }
                }

                // GPS Spoofing Detection
                // 1. Mock location flag
                if (checkInDto.IsMockLocation)
                {
                    isFlagged = true;
                    flagReason = string.IsNullOrEmpty(flagReason) 
                        ? "Mock location detected" 
                        : $"{flagReason}, Mock location detected";
                    fraudScore += 50;
                }

                // 2. Velocity check (impossible travel detection)
                decimal? velocity = null;
                if (checkInDto.Latitude.HasValue && checkInDto.Longitude.HasValue)
                {
                    var previousRecord = await _context.AttendanceRecords
                        .Where(r => r.StudentId == studentId && r.CheckInTime.HasValue)
                        .OrderByDescending(r => r.CheckInTime)
                        .FirstOrDefaultAsync();

                    if (previousRecord != null && 
                        previousRecord.Latitude.HasValue && 
                        previousRecord.Longitude.HasValue &&
                        previousRecord.CheckInTime.HasValue)
                    {
                        var timeDifference = (checkInTime - previousRecord.CheckInTime.Value).TotalHours;
                        
                        if (timeDifference > 0 && timeDifference <= CampusNetworkConstants.MaxTimeDifferenceForVelocityCheck)
                        {
                            var distance = CalculateDistance(
                                previousRecord.Latitude.Value,
                                previousRecord.Longitude.Value,
                                checkInDto.Latitude.Value,
                                checkInDto.Longitude.Value);

                            velocity = (decimal)((double)distance / 1000.0 / timeDifference); // km/h

                            if (velocity > CampusNetworkConstants.MaxRealisticVelocity)
                            {
                                isFlagged = true;
                                flagReason = string.IsNullOrEmpty(flagReason) 
                                    ? "Impossible travel velocity detected" 
                                    : $"{flagReason}, Impossible travel velocity";
                                fraudScore += 40;
                            }
                        }
                    }
                }

                // 3. Device sensor consistency check (if device info provided)
                if (!string.IsNullOrEmpty(checkInDto.DeviceInfo))
                {
                    try
                    {
                        var deviceInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(checkInDto.DeviceInfo);
                        if (deviceInfo != null)
                        {
                            // Check for sensor inconsistencies
                            if (deviceInfo.ContainsKey("sensors"))
                            {
                                var sensors = deviceInfo["sensors"]?.ToString();
                                // Basic check - if sensors are disabled or inconsistent, flag
                                if (sensors != null && sensors.Contains("disabled", StringComparison.OrdinalIgnoreCase))
                                {
                                    fraudScore += 15;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // If device info parsing fails, add small fraud score
                        fraudScore += 5;
                    }
                }

                // Flag if late
                if (isLate)
                {
                    isFlagged = true;
                    flagReason = string.IsNullOrEmpty(flagReason) 
                        ? "Late check-in" 
                        : $"{flagReason}, Late check-in";
                    fraudScore += 10;
                }

                // Create attendance record
                var record = new AttendanceRecord
                {
                    SessionId = sessionId,
                    StudentId = studentId,
                    CheckInTime = checkInTime,
                    Latitude = checkInDto.Latitude,
                    Longitude = checkInDto.Longitude,
                    DistanceFromCenter = distanceFromCenter,
                    IpAddress = clientIpAddress,
                    IsMockLocation = checkInDto.IsMockLocation,
                    Velocity = velocity,
                    DeviceInfo = checkInDto.DeviceInfo,
                    FraudScore = fraudScore,
                    IsFlagged = isFlagged || fraudScore >= CampusNetworkConstants.FraudScoreMedium,
                    FlagReason = flagReason
                };

                await _unitOfWork.AttendanceRecords.AddAsync(record);
                await _unitOfWork.CommitAsync();

                // If flagged, notify instructor
                if (isFlagged || fraudScore >= CampusNetworkConstants.FraudScoreMedium)
                {
                    // TODO: Send notification to instructor
                    // await _notificationService.SendFlaggedAttendanceNotificationAsync(session.InstructorId, record);
                }

                return Response<NoDataDto>.Success(200);
            }
            catch (Exception ex)
            {
                return Response<NoDataDto>.Fail($"Error during check-in: {ex.Message}", 500);
            }
        }

        public async Task<Response<IEnumerable<AttendanceRecordDto>>> GetSessionRecordsAsync(int sessionId)
        {
            try
            {
                var records = await _unitOfWork.AttendanceRecords.GetRecordsBySessionAsync(sessionId);
                var recordDtos = _mapper.Map<IEnumerable<AttendanceRecordDto>>(records);
                return Response<IEnumerable<AttendanceRecordDto>>.Success(recordDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<AttendanceRecordDto>>.Fail($"Error retrieving attendance records: {ex.Message}", 500);
            }
        }

        public async Task<Response<IEnumerable<AttendanceRecordDto>>> GetStudentAttendanceAsync(int studentId)
        {
            try
            {
                var records = await _unitOfWork.AttendanceRecords.GetRecordsByStudentAsync(studentId);
                var recordDtos = _mapper.Map<IEnumerable<AttendanceRecordDto>>(records);
                return Response<IEnumerable<AttendanceRecordDto>>.Success(recordDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<AttendanceRecordDto>>.Fail($"Error retrieving student attendance: {ex.Message}", 500);
            }
        }

        public async Task<Response<AttendanceSessionDto>> GetSessionByIdAsync(int sessionId)
        {
            try
            {
                var session = await _unitOfWork.AttendanceSessions.GetSessionWithRecordsAsync(sessionId);
                if (session == null)
                    return Response<AttendanceSessionDto>.Fail("Session not found", 404);

                var sessionDto = _mapper.Map<AttendanceSessionDto>(session);
                sessionDto.CourseCode = session.Section?.Course?.Code;
                sessionDto.CourseName = session.Section?.Course?.Name;
                sessionDto.InstructorName = session.Instructor?.FullName;

                // Calculate statistics
                if (session.AttendanceRecords != null)
                {
                    var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsBySectionAsync(session.SectionId);
                    var enrolledCount = enrollments.Count(e => e.Status == EnrollmentStatus.Active);
                    
                    sessionDto.TotalStudents = enrolledCount;
                    sessionDto.PresentCount = session.AttendanceRecords.Count(r => r.CheckInTime.HasValue);
                    sessionDto.AbsentCount = enrolledCount - sessionDto.PresentCount;
                }

                return Response<AttendanceSessionDto>.Success(sessionDto, 200);
            }
            catch (Exception ex)
            {
                return Response<AttendanceSessionDto>.Fail($"Error retrieving session: {ex.Message}", 500);
            }
        }

        private string GenerateQrCode(int sectionId, DateTime date)
        {
            return $"QR-{sectionId}-{date:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        /// <summary>
        /// Calculate distance between two coordinates using Haversine formula
        /// Returns distance in meters
        /// </summary>
        private decimal CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double earthRadiusKm = 6371.0;
            
            var dLat = ToRadians((double)(lat2 - lat1));
            var dLon = ToRadians((double)(lon2 - lon1));

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distanceKm = earthRadiusKm * c;
            
            return (decimal)(distanceKm * 1000); // Convert to meters
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        public async Task<Response<NoDataDto>> CloseSessionAsync(int sessionId, string instructorId)
        {
            try
            {
                var session = await _unitOfWork.AttendanceSessions.GetSessionWithRecordsAsync(sessionId);
                if (session == null)
                    return Response<NoDataDto>.Fail("Session not found", 404);

                // Verify instructor owns the session
                if (session.InstructorId != instructorId)
                    return Response<NoDataDto>.Fail("You are not authorized to close this session", 403);

                if (session.Status == AttendanceSessionStatus.Completed || session.Status == AttendanceSessionStatus.Cancelled)
                    return Response<NoDataDto>.Fail("Session is already closed", 400);

                session.Status = AttendanceSessionStatus.Completed;
                _unitOfWork.AttendanceSessions.Update(session);
                await _unitOfWork.CommitAsync();

                return Response<NoDataDto>.Success(200);
            }
            catch (Exception ex)
            {
                return Response<NoDataDto>.Fail($"Error closing session: {ex.Message}", 500);
            }
        }

        public async Task<Response<IEnumerable<AttendanceSessionDto>>> GetMySessionsAsync(string instructorId)
        {
            try
            {
                var sessions = await _context.AttendanceSessions
                    .Where(s => s.InstructorId == instructorId && s.IsActive)
                    .Include(s => s.Section)
                        .ThenInclude(sec => sec.Course)
                    .Include(s => s.Instructor)
                    .OrderByDescending(s => s.Date)
                    .ThenByDescending(s => s.StartTime)
                    .ToListAsync();

                var sessionDtos = new List<AttendanceSessionDto>();
                foreach (var session in sessions)
                {
                    var dto = _mapper.Map<AttendanceSessionDto>(session);
                    dto.CourseCode = session.Section?.Course?.Code;
                    dto.CourseName = session.Section?.Course?.Name;
                    dto.InstructorName = session.Instructor?.FullName;

                    // Calculate statistics
                    var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsBySectionAsync(session.SectionId);
                    dto.TotalStudents = enrollments.Count(e => e.Status == EnrollmentStatus.Active);
                    
                    var records = await _unitOfWork.AttendanceRecords.GetRecordsBySessionAsync(session.Id);
                    dto.PresentCount = records.Count(r => r.CheckInTime.HasValue);
                    dto.AbsentCount = dto.TotalStudents - dto.PresentCount;

                    sessionDtos.Add(dto);
                }

                return Response<IEnumerable<AttendanceSessionDto>>.Success(sessionDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<AttendanceSessionDto>>.Fail($"Error retrieving sessions: {ex.Message}", 500);
            }
        }

        public async Task<Response<AttendanceReportDto>> GetSectionAttendanceReportAsync(int sectionId)
        {
            try
            {
                var section = await _unitOfWork.CourseSections.GetSectionWithDetailsAsync(sectionId);
                if (section == null)
                    return Response<AttendanceReportDto>.Fail("Section not found", 404);

                var sessions = await _context.AttendanceSessions
                    .Where(s => s.SectionId == sectionId && s.IsActive)
                    .Include(s => s.AttendanceRecords)
                    .ToListAsync();

                var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsBySectionAsync(sectionId);
                var activeEnrollments = enrollments.Where(e => e.Status == EnrollmentStatus.Active).ToList();

                var report = new AttendanceReportDto
                {
                    SectionId = sectionId,
                    CourseCode = section.Course?.Code,
                    CourseName = section.Course?.Name,
                    Semester = section.Semester,
                    Year = section.Year,
                    TotalSessions = sessions.Count,
                    TotalStudents = activeEnrollments.Count
                };

                foreach (var enrollment in activeEnrollments)
                {
                    var studentRecords = sessions
                        .SelectMany(s => s.AttendanceRecords ?? new List<AttendanceRecord>())
                        .Where(r => r.StudentId == enrollment.StudentId)
                        .ToList();

                    var presentCount = studentRecords.Count(r => r.CheckInTime.HasValue);
                    var lateCount = studentRecords.Count(r => r.IsFlagged && r.FlagReason != null && r.FlagReason.Contains("Late"));
                    var absentCount = report.TotalSessions - presentCount;

                    var attendancePercentage = report.TotalSessions > 0
                        ? (decimal)presentCount / report.TotalSessions * 100
                        : 0;

                    report.StudentSummaries.Add(new StudentAttendanceSummaryDto
                    {
                        StudentId = enrollment.StudentId,
                        StudentNumber = enrollment.Student?.StudentNumber,
                        StudentName = enrollment.Student?.User?.FullName,
                        PresentCount = presentCount,
                        AbsentCount = absentCount,
                        LateCount = lateCount,
                        AttendancePercentage = attendancePercentage
                    });
                }

                return Response<AttendanceReportDto>.Success(report, 200);
            }
            catch (Exception ex)
            {
                return Response<AttendanceReportDto>.Fail($"Error generating attendance report: {ex.Message}", 500);
            }
        }

        public async Task<Response<QrCodeRefreshDto>> RefreshQrCodeAsync(int sessionId)
        {
            try
            {
                var session = await _unitOfWork.AttendanceSessions.GetByIdAsync(sessionId);
                if (session == null)
                    return Response<QrCodeRefreshDto>.Fail("Session not found", 404);

                // Check if QR code needs refresh (every 5 seconds)
                var shouldRefresh = !session.QrCodeExpiresAt.HasValue || 
                                   DateTime.UtcNow >= session.QrCodeExpiresAt.Value.AddSeconds(-QrCodeRefreshSeconds);

                if (shouldRefresh)
                {
                    // Generate new QR code
                    var newQrCode = GenerateQrCode(session.SectionId, session.Date);
                    var qrCodeGeneratedAt = DateTime.UtcNow;
                    var qrCodeExpiresAt = qrCodeGeneratedAt.AddMinutes(QrCodeExpiryMinutes);

                    session.QrCode = newQrCode;
                    session.QrCodeGeneratedAt = qrCodeGeneratedAt;
                    session.QrCodeExpiresAt = qrCodeExpiresAt;
                    session.UpdatedDate = DateTime.UtcNow;

                    _unitOfWork.AttendanceSessions.Update(session);
                    await _unitOfWork.CommitAsync();

                    return Response<QrCodeRefreshDto>.Success(new QrCodeRefreshDto
                    {
                        QrCode = newQrCode,
                        ExpiresAt = qrCodeExpiresAt
                    }, 200);
                }

                // Return existing QR code
                return Response<QrCodeRefreshDto>.Success(new QrCodeRefreshDto
                {
                    QrCode = session.QrCode ?? "",
                    ExpiresAt = session.QrCodeExpiresAt ?? DateTime.UtcNow.AddMinutes(QrCodeExpiryMinutes)
                }, 200);
            }
            catch (Exception ex)
            {
                return Response<QrCodeRefreshDto>.Fail($"Error refreshing QR code: {ex.Message}", 500);
            }
        }

        public async Task<Response<MyAttendanceDto>> GetMyAttendanceAsync(int studentId)
        {
            try
            {
                var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsByStudentAsync(studentId);
                var activeEnrollments = enrollments
                    .Where(e => e.Status == EnrollmentStatus.Active)
                    .ToList();

                var myAttendance = new MyAttendanceDto();
                int totalSessions = 0;
                int totalPresent = 0;
                int totalAbsent = 0;

                foreach (var enrollment in activeEnrollments)
                {
                    var sessions = await _context.AttendanceSessions
                        .Where(s => s.SectionId == enrollment.SectionId && s.IsActive)
                        .Include(s => s.AttendanceRecords)
                        .ToListAsync();

                    var sectionSessions = sessions.Count;
                    totalSessions += sectionSessions;

                    var records = sessions
                        .SelectMany(s => s.AttendanceRecords ?? new List<AttendanceRecord>())
                        .Where(r => r.StudentId == studentId)
                        .ToList();

                    var presentCount = records.Count(r => r.CheckInTime.HasValue);
                    var lateCount = records.Count(r => r.IsFlagged && r.FlagReason != null && r.FlagReason.Contains("Late"));
                    var absentCount = sectionSessions - presentCount;

                    totalPresent += presentCount;
                    totalAbsent += absentCount;

                    var attendancePercentage = sectionSessions > 0
                        ? (decimal)presentCount / sectionSessions * 100
                        : 0;

                    string status = "Normal";
                    if (attendancePercentage < 70) // 30% absence = critical
                        status = "Critical";
                    else if (attendancePercentage < 80) // 20% absence = warning
                        status = "Warning";

                    myAttendance.Courses.Add(new CourseAttendanceDto
                    {
                        SectionId = enrollment.SectionId,
                        CourseCode = enrollment.Section?.Course?.Code,
                        CourseName = enrollment.Section?.Course?.Name,
                        Semester = enrollment.Section?.Semester ?? "",
                        Year = enrollment.Section?.Year ?? 0,
                        TotalSessions = sectionSessions,
                        PresentCount = presentCount,
                        AbsentCount = absentCount,
                        LateCount = lateCount,
                        AttendancePercentage = attendancePercentage,
                        Status = status
                    });
                }

                myAttendance.TotalSessions = totalSessions;
                myAttendance.TotalPresent = totalPresent;
                myAttendance.TotalAbsent = totalAbsent;
                myAttendance.OverallAttendancePercentage = totalSessions > 0
                    ? (decimal)totalPresent / totalSessions * 100
                    : 0;

                return Response<MyAttendanceDto>.Success(myAttendance, 200);
            }
            catch (Exception ex)
            {
                return Response<MyAttendanceDto>.Fail($"Error retrieving attendance: {ex.Message}", 500);
            }
        }
    }
}

