using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.Constants;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class TranscriptService : ITranscriptService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TranscriptService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TranscriptDto>> GetTranscriptAsync(int studentId)
        {
            try
            {
                var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);
                if (student == null)
                    return Response<TranscriptDto>.Fail("Student not found", 404);

                // Get all completed enrollments
                var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsByStudentAsync(studentId);
                var completedEnrollments = enrollments
                    .Where(e => e.Status == EnrollmentStatus.Completed && 
                           e.LetterGrade != null && 
                           e.LetterGrade != "F")
                    .OrderBy(e => e.Section.Year)
                    .ThenBy(e => e.Section.Semester)
                    .ToList();

                var transcript = new TranscriptDto
                {
                    StudentId = student.Id,
                    StudentNumber = student.StudentNumber,
                    StudentName = student.User?.FullName,
                    DepartmentName = student.Department?.Name,
                    GPA = student.GPA,
                    CGPA = student.CGPA
                };

                foreach (var enrollment in completedEnrollments)
                {
                    var courseDto = new TranscriptCourseDto
                    {
                        CourseCode = enrollment.Section.Course?.Code ?? "",
                        CourseName = enrollment.Section.Course?.Name ?? "",
                        Credits = enrollment.Section.Course?.Credits ?? 0,
                        ECTS = enrollment.Section.Course?.ECTS ?? 0,
                        Semester = enrollment.Section.Semester,
                        Year = enrollment.Section.Year,
                        LetterGrade = enrollment.LetterGrade,
                        GradePoint = enrollment.GradePoint
                    };

                    transcript.Courses.Add(courseDto);
                    transcript.TotalCredits += courseDto.Credits;
                    transcript.TotalECTS += courseDto.ECTS;
                }

                return Response<TranscriptDto>.Success(transcript, 200);
            }
            catch (Exception ex)
            {
                return Response<TranscriptDto>.Fail($"Error retrieving transcript: {ex.Message}", 500);
            }
        }

        public async Task<Response<byte[]>> GenerateTranscriptPdfAsync(int studentId)
        {
            try
            {
                var transcriptResponse = await GetTranscriptAsync(studentId);
                if (!transcriptResponse.IsSuccessful || transcriptResponse.Data == null)
                    return Response<byte[]>.Fail("Failed to retrieve transcript data", 404);

                var transcript = transcriptResponse.Data;

                // Generate simple PDF using basic text formatting
                // For production, use PDFKit or Puppeteer
                var pdfContent = GenerateSimplePdf(transcript);

                return Response<byte[]>.Success(pdfContent, 200);
            }
            catch (Exception ex)
            {
                return Response<byte[]>.Fail($"Error generating PDF: {ex.Message}", 500);
            }
        }

        private byte[] GenerateSimplePdf(TranscriptDto transcript)
        {
            // Simple PDF generation using System.Text
            // For production, install and use QuestPDF, iTextSharp, or PDFKit
            var content = $@"
TRANSCRIPT OF RECORDS
====================

Student Information:
- Student Number: {transcript.StudentNumber}
- Name: {transcript.StudentName}
- Department: {transcript.DepartmentName}
- GPA: {transcript.GPA:F2}
- CGPA: {transcript.CGPA:F2}

Courses:
";

            foreach (var course in transcript.Courses)
            {
                content += $@"
{course.CourseCode} - {course.CourseName}
  Credits: {course.Credits} | ECTS: {course.ECTS}
  Semester: {course.Semester} {course.Year}
  Grade: {course.LetterGrade} (GPA: {course.GradePoint:F2})
";
            }

            content += $@"

Total Credits: {transcript.TotalCredits}
Total ECTS: {transcript.TotalECTS}

Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
";

            // Convert to byte array (simple text-based PDF simulation)
            // In production, use a proper PDF library
            return System.Text.Encoding.UTF8.GetBytes(content);
        }
    }
}

