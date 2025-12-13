using AutoMapper;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class GradeService : IGradeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GradeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
                }

                _unitOfWork.Enrollments.Update(enrollment);
                await _unitOfWork.CommitAsync();

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

                    _unitOfWork.Enrollments.Update(enrollment);
                }

                await _unitOfWork.CommitAsync();
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
                    totalGrade = (midtermGrade.Value * 0.4m) + (finalGrade.Value * 0.6m);
                }
                else
                {
                    totalGrade = finalGrade.Value;
                }

                string letterGrade = totalGrade switch
                {
                    >= 90 => "A",
                    >= 85 => "A-",
                    >= 80 => "B+",
                    >= 75 => "B",
                    >= 70 => "B-",
                    >= 65 => "C+",
                    >= 60 => "C",
                    >= 55 => "C-",
                    >= 50 => "D",
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
                    "A" => 4.0m,
                    "A-" => 3.7m,
                    "B+" => 3.3m,
                    "B" => 3.0m,
                    "B-" => 2.7m,
                    "C+" => 2.3m,
                    "C" => 2.0m,
                    "C-" => 1.7m,
                    "D" => 1.0m,
                    "F" => 0.0m,
                    _ => 0.0m
                };

                return Response<decimal>.Success(gradePoint, 200);
            }
            catch (Exception ex)
            {
                return Response<decimal>.Fail($"Error calculating grade point: {ex.Message}", 500);
            }
        }
    }
}

