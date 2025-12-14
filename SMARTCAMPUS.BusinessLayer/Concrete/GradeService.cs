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
    public class GradeService : IGradeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly CampusContext _context;

        public GradeService(IUnitOfWork unitOfWork, IMapper mapper, CampusContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        public async Task<Response<IEnumerable<GradeDto>>> GetSectionGradesAsync(int sectionId)
        {
            try
            {
                var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsBySectionAsync(sectionId);
                var gradeDtos = _mapper.Map<IEnumerable<GradeDto>>(enrollments);
                return Response<IEnumerable<GradeDto>>.Success(gradeDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<GradeDto>>.Fail($"Error retrieving grades: {ex.Message}", 500);
            }
        }

        public async Task<Response<GradeDto>> GetStudentGradeAsync(int enrollmentId)
        {
            try
            {
                var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(enrollmentId);
                if (enrollment == null)
                    return Response<GradeDto>.Fail("Enrollment not found", 404);

                var gradeDto = _mapper.Map<GradeDto>(enrollment);
                return Response<GradeDto>.Success(gradeDto, 200);
            }
            catch (Exception ex)
            {
                return Response<GradeDto>.Fail($"Error retrieving grade: {ex.Message}", 500);
            }
        }

        public async Task<Response<NoDataDto>> UpdateGradeAsync(int enrollmentId, GradeUpdateDto gradeUpdate)
        {
            try
            {
                var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId);
                if (enrollment == null)
                    return Response<NoDataDto>.Fail("Enrollment not found", 404);

                enrollment.MidtermGrade = gradeUpdate.MidtermGrade;
                enrollment.FinalGrade = gradeUpdate.FinalGrade;

                // Calculate letter grade and grade point
                if (enrollment.FinalGrade.HasValue)
                {
                    var letterGradeResponse = await CalculateLetterGradeAsync(
                        enrollment.MidtermGrade, 
                        enrollment.FinalGrade);
                    
                    if (letterGradeResponse.IsSuccessful && letterGradeResponse.Data != null)
                    {
                        enrollment.LetterGrade = letterGradeResponse.Data;

                        var gradePointResponse = await CalculateGradePointAsync(enrollment.LetterGrade);
                        if (gradePointResponse.IsSuccessful && gradePointResponse.Data != null)
                        {
                            enrollment.GradePoint = gradePointResponse.Data;
                        }
                    }

                    // Update enrollment status to Completed if final grade is entered
                    enrollment.Status = EnrollmentStatus.Completed;
                }

                _unitOfWork.Enrollments.Update(enrollment);
                await _unitOfWork.CommitAsync();

                // Calculate and update student GPA/CGPA
                await UpdateStudentGPAAsync(enrollment.StudentId);

                return Response<NoDataDto>.Success(200);
            }
            catch (Exception ex)
            {
                return Response<NoDataDto>.Fail($"Error updating grade: {ex.Message}", 500);
            }
        }

        public async Task<Response<NoDataDto>> BulkUpdateGradesAsync(int sectionId, GradeBulkUpdateDto grades)
        {
            try
            {
                foreach (var gradeUpdate in grades.Grades)
                {
                    var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(gradeUpdate.EnrollmentId);
                    if (enrollment == null || enrollment.SectionId != sectionId)
                        continue;

                    enrollment.MidtermGrade = gradeUpdate.MidtermGrade;
                    enrollment.FinalGrade = gradeUpdate.FinalGrade;

                    if (enrollment.FinalGrade.HasValue)
                    {
                        var letterGradeResponse = await CalculateLetterGradeAsync(
                            enrollment.MidtermGrade, 
                            enrollment.FinalGrade);
                        
                        if (letterGradeResponse.IsSuccessful && letterGradeResponse.Data != null)
                        {
                            enrollment.LetterGrade = letterGradeResponse.Data;
                            
                            var gradePointResponse = await CalculateGradePointAsync(enrollment.LetterGrade);
                            if (gradePointResponse.IsSuccessful && gradePointResponse.Data != null)
                            {
                                enrollment.GradePoint = gradePointResponse.Data;
                            }
                        }
                    }

                    if (enrollment.FinalGrade.HasValue)
                    {
                        enrollment.Status = EnrollmentStatus.Completed;
                    }

                    _unitOfWork.Enrollments.Update(enrollment);
                }

                await _unitOfWork.CommitAsync();

                // Update GPA/CGPA for all affected students
                var affectedStudentIds = new HashSet<int>();
                foreach (var gradeUpdate in grades.Grades)
                {
                    var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(gradeUpdate.EnrollmentId);
                    if (enrollment != null && enrollment.StudentId > 0)
                    {
                        affectedStudentIds.Add(enrollment.StudentId);
                    }
                }

                foreach (var studentId in affectedStudentIds)
                {
                    await UpdateStudentGPAAsync(studentId);
                }

                return Response<NoDataDto>.Success(200);
            }
            catch (Exception ex)
            {
                return Response<NoDataDto>.Fail($"Error bulk updating grades: {ex.Message}", 500);
            }
        }

        public async Task<Response<string>> CalculateLetterGradeAsync(decimal? midtermGrade, decimal? finalGrade)
        {
            try
            {
                if (!finalGrade.HasValue)
                    return Response<string>.Fail("Final grade is required", 400);

                // Standard grading scale (can be customized)
                // Midterm: 40%, Final: 60%
                decimal totalGrade = 0;
                if (midtermGrade.HasValue)
                {
                    totalGrade = (midtermGrade.Value * Constants.GradeConstants.MidtermWeight) + (finalGrade.Value * Constants.GradeConstants.FinalWeight);
                }
                else
                {
                    totalGrade = finalGrade.Value;
                }

                string letterGrade = totalGrade switch
                {
                    >= Constants.GradeConstants.GradeA => "A",
                    >= Constants.GradeConstants.GradeAMinus => "A-",
                    >= Constants.GradeConstants.GradeBPlus => "B+",
                    >= Constants.GradeConstants.GradeB => "B",
                    >= Constants.GradeConstants.GradeBMinus => "B-",
                    >= Constants.GradeConstants.GradeCPlus => "C+",
                    >= Constants.GradeConstants.GradeC => "C",
                    >= Constants.GradeConstants.GradeCMinus => "C-",
                    >= Constants.GradeConstants.GradeD => "D",
                    _ => "F"
                };

                return Response<string>.Success(letterGrade, 200);
            }
            catch (Exception ex)
            {
                return Response<string>.Fail($"Error calculating letter grade: {ex.Message}", 500);
            }
        }

        public async Task<Response<decimal>> CalculateGradePointAsync(string letterGrade)
        {
            try
            {
                if (string.IsNullOrEmpty(letterGrade))
                    return Response<decimal>.Fail("Letter grade is required", 400);

                decimal gradePoint = letterGrade.ToUpper() switch
                {
                    "A" => Constants.GradePoints.A,
                    "A-" => Constants.GradePoints.AMinus,
                    "B+" => Constants.GradePoints.BPlus,
                    "B" => Constants.GradePoints.B,
                    "B-" => Constants.GradePoints.BMinus,
                    "C+" => Constants.GradePoints.CPlus,
                    "C" => Constants.GradePoints.C,
                    "C-" => Constants.GradePoints.CMinus,
                    "D" => Constants.GradePoints.D,
                    "F" => Constants.GradePoints.F,
                    _ => Constants.GradePoints.F
                };

                return Response<decimal>.Success(gradePoint, 200);
            }
            catch (Exception ex)
            {
                return Response<decimal>.Fail($"Error calculating grade point: {ex.Message}", 500);
            }
        }

        public async Task<Response<IEnumerable<GradeDto>>> GetMyGradesAsync(int studentId)
        {
            try
            {
                var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsByStudentAsync(studentId);
                var gradeDtos = _mapper.Map<IEnumerable<GradeDto>>(enrollments);
                return Response<IEnumerable<GradeDto>>.Success(gradeDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<GradeDto>>.Fail($"Error retrieving grades: {ex.Message}", 500);
            }
        }

        public async Task<Response<NoDataDto>> CreateGradeAsync(int enrollmentId, GradeUpdateDto gradeUpdate, string instructorId)
        {
            try
            {
                var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(enrollmentId);
                if (enrollment == null)
                    return Response<NoDataDto>.Fail("Enrollment not found", 404);

                // Validate instructor teaches the section
                var section = await _unitOfWork.CourseSections.GetSectionWithDetailsAsync(enrollment.SectionId);
                if (section == null)
                    return Response<NoDataDto>.Fail("Section not found", 404);

                if (section.InstructorId != instructorId)
                    return Response<NoDataDto>.Fail("You are not authorized to enter grades for this section", 403);

                enrollment.MidtermGrade = gradeUpdate.MidtermGrade;
                enrollment.FinalGrade = gradeUpdate.FinalGrade;

                // Calculate letter grade and grade point
                if (enrollment.FinalGrade.HasValue)
                {
                    var letterGradeResponse = await CalculateLetterGradeAsync(
                        enrollment.MidtermGrade,
                        enrollment.FinalGrade);

                    if (letterGradeResponse.IsSuccessful && letterGradeResponse.Data != null)
                    {
                        enrollment.LetterGrade = letterGradeResponse.Data;

                        var gradePointResponse = await CalculateGradePointAsync(enrollment.LetterGrade);
                        if (gradePointResponse.IsSuccessful && gradePointResponse.Data != null)
                        {
                            enrollment.GradePoint = gradePointResponse.Data;
                        }
                    }

                    // Update enrollment status to Completed if final grade is entered
                    enrollment.Status = EnrollmentStatus.Completed;
                }

                _unitOfWork.Enrollments.Update(enrollment);
                await _unitOfWork.CommitAsync();

                // Calculate and update student GPA/CGPA
                await UpdateStudentGPAAsync(enrollment.StudentId);

                // TODO: Send notification to student
                // await _notificationService.SendGradeNotificationAsync(enrollment.StudentId, enrollment);

                return Response<NoDataDto>.Success(201);
            }
            catch (Exception ex)
            {
                return Response<NoDataDto>.Fail($"Error creating grade: {ex.Message}", 500);
            }
        }

        public async Task<Response<decimal>> CalculateGPAAsync(int studentId)
        {
            try
            {
                var enrollments = await _context.Enrollments
                    .Where(e => e.StudentId == studentId
                        && e.Status == EnrollmentStatus.Completed
                        && e.LetterGrade != null
                        && e.LetterGrade != "F"
                        && e.IsActive)
                    .Include(e => e.Section)
                        .ThenInclude(s => s.Course)
                    .ToListAsync();

                if (!enrollments.Any())
                    return Response<decimal>.Success(0, 200);

                decimal totalPoints = 0;
                int totalCredits = 0;

                foreach (var enrollment in enrollments)
                {
                    if (enrollment.GradePoint.HasValue && enrollment.Section?.Course != null)
                    {
                        totalPoints += enrollment.GradePoint.Value * enrollment.Section.Course.Credits;
                        totalCredits += enrollment.Section.Course.Credits;
                    }
                }

                var gpa = totalCredits > 0 ? totalPoints / totalCredits : 0;
                return Response<decimal>.Success(gpa, 200);
            }
            catch (Exception ex)
            {
                return Response<decimal>.Fail($"Error calculating GPA: {ex.Message}", 500);
            }
        }

        public async Task<Response<decimal>> CalculateCGPAAsync(int studentId)
        {
            try
            {
                // CGPA is the same as GPA for all completed courses
                return await CalculateGPAAsync(studentId);
            }
            catch (Exception ex)
            {
                return Response<decimal>.Fail($"Error calculating CGPA: {ex.Message}", 500);
            }
        }

        private async Task UpdateStudentGPAAsync(int studentId)
        {
            var gpaResponse = await CalculateGPAAsync(studentId);
            if (gpaResponse.IsSuccessful && gpaResponse.Data != null)
            {
                var student = await _unitOfWork.Students.GetByIdAsync(studentId);
                if (student != null)
                {
                    student.GPA = (double)gpaResponse.Data;
                    student.CGPA = (double)gpaResponse.Data; // CGPA = GPA for all courses
                    _unitOfWork.Students.Update(student);
                    await _unitOfWork.CommitAsync();
                }
            }
        }
    }
}



