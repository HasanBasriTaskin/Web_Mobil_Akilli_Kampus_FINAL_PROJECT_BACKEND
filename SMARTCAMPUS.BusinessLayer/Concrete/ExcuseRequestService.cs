using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Constants;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class ExcuseRequestService : IExcuseRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly CampusContext _context;

        public ExcuseRequestService(IUnitOfWork unitOfWork, IMapper mapper, CampusContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        public async Task<Response<ExcuseRequestDto>> CreateExcuseRequestAsync(int studentId, ExcuseRequestCreateDto createDto)
        {
            try
            {
                // Verify session exists
                var session = await _unitOfWork.AttendanceSessions.GetSessionWithRecordsAsync(createDto.SessionId);
                if (session == null)
                    return Response<ExcuseRequestDto>.Fail("Session not found", 404);

                // Verify student is enrolled in the section
                var enrollment = await _unitOfWork.Enrollments
                    .GetEnrollmentByStudentAndSectionAsync(studentId, session.SectionId);
                
                if (enrollment == null)
                    return Response<ExcuseRequestDto>.Fail("Student is not enrolled in this section", 403);

                // Check if excuse request already exists
                var existingRequest = await _context.ExcuseRequests
                    .FirstOrDefaultAsync(e => e.StudentId == studentId && 
                                             e.SessionId == createDto.SessionId && 
                                             e.IsActive);

                if (existingRequest != null)
                    return Response<ExcuseRequestDto>.Fail("Excuse request already exists for this session", 400);

                var excuseRequest = new ExcuseRequest
                {
                    StudentId = studentId,
                    SessionId = createDto.SessionId,
                    Reason = createDto.Reason,
                    DocumentUrl = createDto.DocumentUrl,
                    Status = ExcuseRequestStatus.Pending
                };

                await _unitOfWork.ExcuseRequests.AddAsync(excuseRequest);
                await _unitOfWork.CommitAsync();

                // TODO: Notify instructor
                // await _notificationService.SendExcuseRequestNotificationAsync(session.InstructorId, excuseRequest);

                var excuseRequestDto = _mapper.Map<ExcuseRequestDto>(excuseRequest);
                excuseRequestDto.StudentNumber = enrollment.Student?.StudentNumber;
                excuseRequestDto.StudentName = enrollment.Student?.User?.FullName;
                excuseRequestDto.SessionDate = session.Date;
                excuseRequestDto.CourseCode = session.Section?.Course?.Code;

                return Response<ExcuseRequestDto>.Success(excuseRequestDto, 201);
            }
            catch (Exception ex)
            {
                return Response<ExcuseRequestDto>.Fail($"Error creating excuse request: {ex.Message}", 500);
            }
        }

        public async Task<Response<IEnumerable<ExcuseRequestDto>>> GetExcuseRequestsAsync(string? instructorId = null)
        {
            try
            {
                var query = _context.ExcuseRequests
                    .Where(e => e.IsActive)
                    .Include(e => e.Student)
                        .ThenInclude(s => s.User)
                    .Include(e => e.Session)
                        .ThenInclude(s => s.Section)
                            .ThenInclude(sec => sec.Course)
                    .AsQueryable();

                // If instructorId provided, filter by instructor's sections
                if (!string.IsNullOrEmpty(instructorId))
                {
                    query = query.Where(e => e.Session.InstructorId == instructorId);
                }

                var excuseRequests = await query
                    .OrderByDescending(e => e.CreatedDate)
                    .ToListAsync();

                var excuseRequestDtos = _mapper.Map<IEnumerable<ExcuseRequestDto>>(excuseRequests);
                foreach (var dto in excuseRequestDtos)
                {
                    var request = excuseRequests.FirstOrDefault(r => r.Id == dto.Id);
                    if (request != null)
                    {
                        dto.StudentNumber = request.Student?.StudentNumber;
                        dto.StudentName = request.Student?.User?.FullName;
                        dto.SessionDate = request.Session?.Date;
                        dto.CourseCode = request.Session?.Section?.Course?.Code;
                    }
                }

                return Response<IEnumerable<ExcuseRequestDto>>.Success(excuseRequestDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<ExcuseRequestDto>>.Fail($"Error retrieving excuse requests: {ex.Message}", 500);
            }
        }

        public async Task<Response<NoDataDto>> ApproveExcuseRequestAsync(int requestId, string instructorId, string? notes = null)
        {
            try
            {
                var request = await _unitOfWork.ExcuseRequests.GetRequestWithDetailsAsync(requestId);
                if (request == null)
                    return Response<NoDataDto>.Fail("Excuse request not found", 404);

                // Verify instructor owns the session
                if (request.Session?.InstructorId != instructorId)
                    return Response<NoDataDto>.Fail("You are not authorized to approve this request", 403);

                if (request.Status != ExcuseRequestStatus.Pending)
                    return Response<NoDataDto>.Fail("Request has already been reviewed", 400);

                request.Status = ExcuseRequestStatus.Approved;
                request.ReviewedBy = instructorId;
                request.ReviewedAt = DateTime.UtcNow;
                request.Notes = notes;
                request.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.ExcuseRequests.Update(request);
                await _unitOfWork.CommitAsync();

                // TODO: Notify student
                // await _notificationService.SendExcuseRequestApprovalNotificationAsync(request.StudentId, request);

                return Response<NoDataDto>.Success(200);
            }
            catch (Exception ex)
            {
                return Response<NoDataDto>.Fail($"Error approving excuse request: {ex.Message}", 500);
            }
        }

        public async Task<Response<NoDataDto>> RejectExcuseRequestAsync(int requestId, string instructorId, string? notes = null)
        {
            try
            {
                var request = await _unitOfWork.ExcuseRequests.GetRequestWithDetailsAsync(requestId);
                if (request == null)
                    return Response<NoDataDto>.Fail("Excuse request not found", 404);

                // Verify instructor owns the session
                if (request.Session?.InstructorId != instructorId)
                    return Response<NoDataDto>.Fail("You are not authorized to reject this request", 403);

                if (request.Status != ExcuseRequestStatus.Pending)
                    return Response<NoDataDto>.Fail("Request has already been reviewed", 400);

                request.Status = ExcuseRequestStatus.Rejected;
                request.ReviewedBy = instructorId;
                request.ReviewedAt = DateTime.UtcNow;
                request.Notes = notes;
                request.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.ExcuseRequests.Update(request);
                await _unitOfWork.CommitAsync();

                // TODO: Notify student
                // await _notificationService.SendExcuseRequestRejectionNotificationAsync(request.StudentId, request);

                return Response<NoDataDto>.Success(200);
            }
            catch (Exception ex)
            {
                return Response<NoDataDto>.Fail($"Error rejecting excuse request: {ex.Message}", 500);
            }
        }
    }
}

