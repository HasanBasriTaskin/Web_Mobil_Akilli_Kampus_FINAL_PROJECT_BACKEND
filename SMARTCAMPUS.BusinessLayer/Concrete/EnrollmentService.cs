using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EnrollmentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
                
                if (existingEnrollment != null && existingEnrollment.Status == "Active")
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

                // Check capacity (atomic operation)
                if (section.EnrolledCount >= section.Capacity)
                {
                    response.Message = "Section is full";
                    return Response<EnrollmentResponseDto>.Success(response, 400);
                }

                // Check prerequisites
                var prerequisitesMet = await _unitOfWork.Courses
                    .CheckPrerequisiteAsync(section.CourseId, studentId);
                
                if (!prerequisitesMet)
                {
                    var course = await _unitOfWork.Courses.GetCourseWithPrerequisitesAsync(section.CourseId);
                    response.MissingPrerequisites = course?.Prerequisites
                        .Select(p => p.PrerequisiteCourse.Code)
                        .ToList() ?? new List<string>();
                    response.Message = "Prerequisites not met";
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
                    Status = "Active",
                    EnrollmentDate = DateTime.UtcNow
                };

                await _unitOfWork.Enrollments.AddAsync(enrollment);

                // Atomic increment of enrolled count
                section.EnrolledCount++;
                _unitOfWork.CourseSections.Update(section);

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

                if (enrollment.Status != "Active")
                    return Response<NoDataDto>.Fail("Cannot drop non-active enrollment", 400);

                enrollment.Status = "Dropped";
                _unitOfWork.Enrollments.Update(enrollment);

                // Decrement enrolled count
                var section = await _unitOfWork.CourseSections.GetByIdAsync(enrollment.SectionId);
                if (section != null)
                {
                    section.EnrolledCount = Math.Max(0, section.EnrolledCount - 1);
                    _unitOfWork.CourseSections.Update(section);
                }

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
    }
}



