using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly CampusContext _context;
        private const int DropPeriodWeeks = 4;

        public EnrollmentService(IUnitOfWork unitOfWork, IMapper mapper, CampusContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        public async Task<Response<EnrollmentResponseDto>> EnrollAsync(int studentId, EnrollmentRequestDto request)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var response = new EnrollmentResponseDto { Success = false };

                // Check if already enrolled
                var existingEnrollment = await _unitOfWork.Enrollments
                    .GetEnrollmentByStudentAndSectionAsync(studentId, request.SectionId);
                
                if (existingEnrollment != null && existingEnrollment.Status == SMARTCAMPUS.EntityLayer.Constants.EnrollmentStatus.Active)
                {
                    response.Message = "Already enrolled in this section";
                    return Response<EnrollmentResponseDto>.Success(response, 400);
                }

                // Get section details
                var section = await _unitOfWork.CourseSections.GetSectionWithDetailsAsync(request.SectionId);
                if (section == null)
                {
                    response.Message = "Section not found";
                    return Response<EnrollmentResponseDto>.Success(response, 404);
                }

                // Atomic capacity check and update using SQL
                var affectedRows = await _context.Database.ExecuteSqlRawAsync(
                    $"UPDATE CourseSections SET EnrolledCount = EnrolledCount + 1 WHERE Id = {request.SectionId} AND EnrolledCount < Capacity AND IsActive = 1");

                if (affectedRows == 0)
                {
                    response.Message = "Section is full or not available";
                    return Response<EnrollmentResponseDto>.Success(response, 400);
                }

                // Refresh section data after atomic update
                section = await _unitOfWork.CourseSections.GetSectionWithDetailsAsync(request.SectionId);
                if (section == null)
                {
                    await transaction.RollbackAsync();
                    response.Message = "Section not found after capacity update";
                    return Response<EnrollmentResponseDto>.Success(response, 404);
                }

                // Check prerequisites (recursive)
                var prerequisitesMet = await _unitOfWork.Courses
                    .CheckPrerequisiteAsync(section.CourseId, studentId);
                
                if (!prerequisitesMet)
                {
                    // Get all missing prerequisites recursively
                    var missingPrereqs = await GetMissingPrerequisitesRecursiveAsync(section.CourseId, studentId, new HashSet<int>());
                    response.MissingPrerequisites = missingPrereqs;
                    response.Message = "Prerequisites not met";
                    
                    // Rollback capacity increment
                    await _context.Database.ExecuteSqlRawAsync(
                        $"UPDATE CourseSections SET EnrolledCount = EnrolledCount - 1 WHERE Id = {request.SectionId}");
                    await transaction.RollbackAsync();
                    return Response<EnrollmentResponseDto>.Success(response, 400);
                }

                // Check schedule conflict
                var hasConflict = await _unitOfWork.CourseSections
                    .HasScheduleConflictAsync(studentId, request.SectionId, section.Semester, section.Year);
                
                if (hasConflict)
                {
                    response.Message = "Schedule conflict detected";
                    response.Conflicts = new List<string> { section.Course?.Code ?? "Unknown" };
                    return Response<EnrollmentResponseDto>.Success(response, 400);
                }

                // Create enrollment
                var enrollment = new Enrollment
                {
                    StudentId = studentId,
                    SectionId = request.SectionId,
                    Status = SMARTCAMPUS.EntityLayer.Constants.EnrollmentStatus.Active,
                    EnrollmentDate = DateTime.UtcNow
                };

                await _unitOfWork.Enrollments.AddAsync(enrollment);
                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();

                var enrollmentDto = _mapper.Map<EnrollmentDto>(enrollment);
                response.Success = true;
                response.Message = "Successfully enrolled";
                response.Enrollment = enrollmentDto;

                return Response<EnrollmentResponseDto>.Success(response, 201);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Response<EnrollmentResponseDto>.Fail($"Error enrolling: {ex.Message}", 500);
            }
        }

        public async Task<Response<NoDataDto>> DropCourseAsync(int studentId, int enrollmentId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(enrollmentId);
                if (enrollment == null || enrollment.StudentId != studentId)
                    return Response<NoDataDto>.Fail("Enrollment not found", 404);

                if (enrollment.Status != EnrollmentStatus.Active)
                    return Response<NoDataDto>.Fail("Cannot drop non-active enrollment", 400);

                // Check drop period (first 4 weeks)
                var enrollmentDate = enrollment.EnrollmentDate;
                var weeksSinceEnrollment = (DateTime.UtcNow - enrollmentDate).TotalDays / 7;
                
                if (weeksSinceEnrollment > DropPeriodWeeks)
                {
                    return Response<NoDataDto>.Fail($"Drop period has expired. You can only drop courses within the first {DropPeriodWeeks} weeks.", 400);
                }

                enrollment.Status = EnrollmentStatus.Dropped;
                _unitOfWork.Enrollments.Update(enrollment);

                // Atomic decrement of enrolled count
                await _context.Database.ExecuteSqlRawAsync(
                    $"UPDATE CourseSections SET EnrolledCount = CASE WHEN EnrolledCount > 0 THEN EnrolledCount - 1 ELSE 0 END WHERE Id = {enrollment.SectionId}");

                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();

                return Response<NoDataDto>.Success(200);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Response<NoDataDto>.Fail($"Error dropping course: {ex.Message}", 500);
            }
        }

        public async Task<Response<IEnumerable<EnrollmentDto>>> GetStudentEnrollmentsAsync(int studentId)
        {
            try
            {
                var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsByStudentAsync(studentId);
                var enrollmentDtos = _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
                return Response<IEnumerable<EnrollmentDto>>.Success(enrollmentDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<EnrollmentDto>>.Fail($"Error retrieving enrollments: {ex.Message}", 500);
            }
        }

        public async Task<Response<IEnumerable<EnrollmentDto>>> GetSectionEnrollmentsAsync(int sectionId)
        {
            try
            {
                var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsBySectionAsync(sectionId);
                var enrollmentDtos = _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
                return Response<IEnumerable<EnrollmentDto>>.Success(enrollmentDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<EnrollmentDto>>.Fail($"Error retrieving enrollments: {ex.Message}", 500);
            }
        }

        public async Task<Response<bool>> CheckPrerequisitesAsync(int courseId, int studentId)
        {
            try
            {
                var result = await _unitOfWork.Courses.CheckPrerequisiteAsync(courseId, studentId);
                return Response<bool>.Success(result, 200);
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error checking prerequisites: {ex.Message}", 500);
            }
        }

        public async Task<Response<bool>> CheckScheduleConflictAsync(int studentId, int sectionId)
        {
            try
            {
                var section = await _unitOfWork.CourseSections.GetByIdAsync(sectionId);
                if (section == null)
                    return Response<bool>.Fail("Section not found", 404);

                var hasConflict = await _unitOfWork.CourseSections
                    .HasScheduleConflictAsync(studentId, sectionId, section.Semester, section.Year);
                
                return Response<bool>.Success(hasConflict, 200);
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error checking schedule conflict: {ex.Message}", 500);
            }
        }

        private async Task<List<string>> GetMissingPrerequisitesRecursiveAsync(int courseId, int studentId, HashSet<int> visited)
        {
            var missingPrereqs = new List<string>();

            if (visited.Contains(courseId))
                return missingPrereqs;

            visited.Add(courseId);

            var course = await _unitOfWork.Courses.GetCourseWithPrerequisitesAsync(courseId);
            if (course == null || !course.Prerequisites.Any())
                return missingPrereqs;

            var studentEnrollments = await _context.Enrollments
                .Where(e => e.StudentId == studentId
                    && (e.Status == EnrollmentStatus.Completed || e.LetterGrade != "F")
                    && e.IsActive)
                .Include(e => e.Section)
                    .ThenInclude(s => s.Course)
                .ToListAsync();

            var completedCourseIds = studentEnrollments
                .Select(e => e.Section.CourseId)
                .Distinct()
                .ToList();

            foreach (var prereq in course.Prerequisites)
            {
                if (!completedCourseIds.Contains(prereq.PrerequisiteCourseId))
                {
                    missingPrereqs.Add(prereq.PrerequisiteCourse.Code);
                }

                // Recursive check
                var nestedMissing = await GetMissingPrerequisitesRecursiveAsync(prereq.PrerequisiteCourseId, studentId, visited);
                missingPrereqs.AddRange(nestedMissing);
            }

            return missingPrereqs.Distinct().ToList();
        }
    }
}



