using AutoMapper;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AttendanceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<AttendanceSessionDto>> CreateSessionAsync(AttendanceSessionDto sessionDto)
        {
            try
            {
                // Verify section exists
                var section = await _unitOfWork.CourseSections.GetSectionWithDetailsAsync(sessionDto.SectionId);
                if (section == null)
                    return Response<AttendanceSessionDto>.Fail("Section not found", 404);

                // Generate QR code if not provided
                var qrCode = sessionDto.QrCode ?? GenerateQrCode(sessionDto.SectionId, sessionDto.Date);

                var session = new AttendanceSession
                {
                    SectionId = sessionDto.SectionId,
                    InstructorId = sessionDto.InstructorId,
                    Date = sessionDto.Date,
                    StartTime = sessionDto.StartTime,
                    EndTime = sessionDto.EndTime,
                    Latitude = sessionDto.Latitude,
                    Longitude = sessionDto.Longitude,
                    GeofenceRadius = sessionDto.GeofenceRadius,
                    QrCode = qrCode,
                    Status = "Scheduled"
                };

                await _unitOfWork.AttendanceSessions.AddAsync(session);
                await _unitOfWork.CommitAsync();

                var resultDto = _mapper.Map<AttendanceSessionDto>(session);
                resultDto.CourseCode = section.Course?.Code;
                resultDto.CourseName = section.Course?.Name;
                resultDto.InstructorName = section.Instructor?.FullName;

                return Response<AttendanceSessionDto>.Success(resultDto, 201);
            }
            catch (Exception ex)
            {
                return Response<AttendanceSessionDto>.Fail($"Error creating attendance session: {ex.Message}", 500);
            }
        }

        public async Task<Response<NoDataDto>> CheckInAsync(int studentId, AttendanceCheckInDto checkInDto)
        {
            try
            {
                // Get session with details
                var session = await _unitOfWork.AttendanceSessions.GetSessionWithRecordsAsync(checkInDto.SessionId);
                if (session == null)
                    return Response<NoDataDto>.Fail("Attendance session not found", 404);

                // Check if session is active
                if (session.Status != "Active" && session.Status != "Scheduled")
                    return Response<NoDataDto>.Fail("Session is not available for check-in", 400);

                // Check if student is enrolled in the section
                var enrollment = await _unitOfWork.Enrollments
                    .GetEnrollmentByStudentAndSectionAsync(studentId, session.SectionId);
                
                if (enrollment == null || enrollment.Status != "Active")
                    return Response<NoDataDto>.Fail("Student is not enrolled in this section", 403);

                // Check if already checked in
                var existingRecord = await _unitOfWork.AttendanceRecords
                    .GetRecordBySessionAndStudentAsync(checkInDto.SessionId, studentId);
                
                if (existingRecord != null)
                    return Response<NoDataDto>.Fail("Already checked in", 400);

                // Verify QR code if provided
                if (!string.IsNullOrEmpty(checkInDto.QrCode) && 
                    !string.IsNullOrEmpty(session.QrCode) &&
                    checkInDto.QrCode != session.QrCode)
                {
                    return Response<NoDataDto>.Fail("Invalid QR code", 400);
                }

                var checkInTime = DateTime.UtcNow;
                var sessionDateTime = session.Date.Add(session.StartTime);
                var isLate = checkInTime > sessionDateTime.AddMinutes(15); // 15 minutes grace period

                decimal? distanceFromCenter = null;
                bool isFlagged = false;
                string? flagReason = null;

                // Geofencing check
                if (session.Latitude.HasValue && session.Longitude.HasValue &&
                    checkInDto.Latitude.HasValue && checkInDto.Longitude.HasValue)
                {
                    distanceFromCenter = CalculateDistance(
                        session.Latitude.Value,
                        session.Longitude.Value,
                        checkInDto.Latitude.Value,
                        checkInDto.Longitude.Value);

                    if (session.GeofenceRadius.HasValue && 
                        distanceFromCenter > session.GeofenceRadius.Value)
                    {
                        isFlagged = true;
                        flagReason = "Outside geofence";
                    }
                }

                // Flag if late
                if (isLate)
                {
                    isFlagged = true;
                    flagReason = string.IsNullOrEmpty(flagReason) 
                        ? "Late check-in" 
                        : $"{flagReason}, Late check-in";
                }

                // Create attendance record
                var record = new AttendanceRecord
                {
                    SessionId = checkInDto.SessionId,
                    StudentId = studentId,
                    CheckInTime = checkInTime,
                    Latitude = checkInDto.Latitude,
                    Longitude = checkInDto.Longitude,
                    DistanceFromCenter = distanceFromCenter,
                    IsFlagged = isFlagged,
                    FlagReason = flagReason
                };

                await _unitOfWork.AttendanceRecords.AddAsync(record);
                await _unitOfWork.CommitAsync();

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
                    var enrolledCount = enrollments.Count(e => e.Status == "Active");
                    
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
    }
}

