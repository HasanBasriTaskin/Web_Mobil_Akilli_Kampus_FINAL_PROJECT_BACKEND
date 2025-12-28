using System.Linq;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Grade;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class GradeManager : IGradeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GradeManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<IEnumerable<StudentGradeDto>>> GetMyGradesAsync(int studentId)
        {
            var enrollments = await _unitOfWork.Enrollments
                .GetListAsync(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Enrolled);

            var grades = new List<StudentGradeDto>();

            foreach (var enrollment in enrollments)
            {
                var section = await _unitOfWork.CourseSections.GetByIdAsync(enrollment.SectionId);
                if (section == null) continue;

                var course = await _unitOfWork.Courses.GetByIdAsync(section.CourseId);
                if (course == null) continue;

                grades.Add(new StudentGradeDto
                {
                    CourseCode = course.Code,
                    CourseName = course.Name,
                    Credits = course.Credits,
                    Semester = section.Semester,
                    Year = section.Year,
                    MidtermGrade = enrollment.MidtermGrade,
                    FinalGrade = enrollment.FinalGrade,
                    LetterGrade = enrollment.LetterGrade,
                    GradePoint = CalculateGradePoint(enrollment.LetterGrade)
                });
            }

            return Response<IEnumerable<StudentGradeDto>>.Success(grades, 200);
        }

        public async Task<Response<TranscriptDto>> GetTranscriptAsync(int studentId)
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);
            if (student == null)
                return Response<TranscriptDto>.Fail("Student not found", 404);

            var department = await _unitOfWork.Departments.GetByIdAsync(student.DepartmentId);
            var gradesResult = await GetMyGradesAsync(studentId);
            var grades = gradesResult.Data?.ToList() ?? new List<StudentGradeDto>();

            var semesters = grades
                .GroupBy(g => new { g.Semester, g.Year })
                .Select(group => new SemesterGradesDto
                {
                    Semester = group.Key.Semester,
                    Year = group.Key.Year,
                    Credits = group.Sum(g => g.Credits),
                    GPA = CalculateSemesterGPA(group.ToList()),
                    Courses = group.ToList()
                })
                .OrderBy(s => s.Year)
                .ThenBy(s => s.Semester)
                .ToList();

            var transcript = new TranscriptDto
            {
                StudentNumber = student.StudentNumber,
                StudentName = student.User?.FullName ?? "Unknown",
                DepartmentName = department?.Name ?? "Unknown",
                TotalCredits = grades.Sum(g => g.Credits),
                TotalECTS = grades.Sum(g => g.Credits),
                CGPA = CalculateCGPA(grades),
                Semesters = semesters
            };

            return Response<TranscriptDto>.Success(transcript, 200);
        }

        public async Task<byte[]> GenerateTranscriptPdfAsync(int studentId)
        {
            var transcript = await GetTranscriptAsync(studentId);
            var content = $"Transcript for {transcript.Data?.StudentName ?? "Unknown"}\nCGPA: {transcript.Data?.CGPA:F2}";
            return System.Text.Encoding.UTF8.GetBytes(content);
        }

        public async Task<Response<NoDataDto>> EnterGradeAsync(int instructorId, GradeEntryDto dto)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(dto.EnrollmentId);
            if (enrollment == null)
                return Response<NoDataDto>.Fail("Enrollment not found", 404);

            var section = await _unitOfWork.CourseSections.GetByIdAsync(enrollment.SectionId);
            if (section == null)
                return Response<NoDataDto>.Fail("Section not found", 404);

            if (section.InstructorId != instructorId)
                return Response<NoDataDto>.Fail("You are not authorized to grade this section", 403);

            if (dto.MidtermGrade.HasValue)
                enrollment.MidtermGrade = dto.MidtermGrade;

            if (dto.FinalGrade.HasValue)
                enrollment.FinalGrade = dto.FinalGrade;

            if (enrollment.MidtermGrade.HasValue && enrollment.FinalGrade.HasValue)
            {
                enrollment.LetterGrade = CalculateLetterGrade(enrollment.MidtermGrade.Value, enrollment.FinalGrade.Value);
            }

            _unitOfWork.Enrollments.Update(enrollment);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> EnterGradesBatchAsync(int instructorId, List<GradeEntryDto> dtos)
        {
            foreach (var dto in dtos)
            {
                var result = await EnterGradeAsync(instructorId, dto);
                if (!result.IsSuccessful)
                    return result;
            }

            return Response<NoDataDto>.Success(200);
        }

        private static string CalculateLetterGrade(double midterm, double final)
        {
            var total = (midterm * 0.4) + (final * 0.6);

            return total switch
            {
                >= 90 => "AA",
                >= 85 => "BA",
                >= 80 => "BB",
                >= 75 => "CB",
                >= 70 => "CC",
                >= 65 => "DC",
                >= 60 => "DD",
                _ => "FF"
            };
        }

        private static double? CalculateGradePoint(string? letterGrade)
        {
            return letterGrade switch
            {
                "AA" => 4.0,
                "BA" => 3.5,
                "BB" => 3.0,
                "CB" => 2.5,
                "CC" => 2.0,
                "DC" => 1.5,
                "DD" => 1.0,
                "FF" => 0.0,
                _ => null
            };
        }

        private static double CalculateSemesterGPA(List<StudentGradeDto> grades)
        {
            var gradedCourses = grades.Where(g => g.GradePoint.HasValue).ToList();
            if (gradedCourses.Count == 0) return 0;

            var totalPoints = gradedCourses.Sum(g => g.GradePoint!.Value * g.Credits);
            var totalCredits = gradedCourses.Sum(g => g.Credits);

            return totalCredits > 0 ? totalPoints / totalCredits : 0;
        }

        private static double CalculateCGPA(List<StudentGradeDto> grades)
        {
            return CalculateSemesterGPA(grades);
        }
    }
}
