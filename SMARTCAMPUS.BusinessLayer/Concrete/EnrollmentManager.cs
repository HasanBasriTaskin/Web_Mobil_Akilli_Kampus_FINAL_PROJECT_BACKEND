using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Enrollment;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using System.Text.Json;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class EnrollmentManager : IEnrollmentService
    {
        private readonly IEnrollmentDal _enrollmentDal;
        private readonly ICourseSectionDal _sectionDal;
        private readonly ICoursePrerequisiteDal _prerequisiteDal;
        private readonly CampusContext _context;

        public EnrollmentManager(
            IEnrollmentDal enrollmentDal,
            ICourseSectionDal sectionDal,
            ICoursePrerequisiteDal prerequisiteDal,
            CampusContext context)
        {
            _enrollmentDal = enrollmentDal;
            _sectionDal = sectionDal;
            _prerequisiteDal = prerequisiteDal;
            _context = context;
        }

        public async Task<Response<EnrollmentDto>> EnrollInCourseAsync(int studentId, CreateEnrollmentDto dto)
        {
            // Get section with course info
            var section = await _sectionDal.GetSectionWithDetailsAsync(dto.SectionId);
            if (section == null)
                return Response<EnrollmentDto>.Fail("Section not found", 404);

            // Check capacity
            if (section.EnrolledCount >= section.Capacity)
                return Response<EnrollmentDto>.Fail("Section is full", 400);

            // Check prerequisites
            var prereqResult = await CheckPrerequisitesAsync(studentId, section.CourseId);
            if (!prereqResult.IsSuccessful)
                return Response<EnrollmentDto>.Fail(prereqResult.Errors!, 400);

            // Check schedule conflicts
            var conflictResult = await CheckScheduleConflictAsync(studentId, dto.SectionId);
            if (!conflictResult.IsSuccessful)
                return Response<EnrollmentDto>.Fail(conflictResult.Errors!, 400);

            // Check if already enrolled
            var existingEnrollment = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.SectionId == dto.SectionId);
            if (existingEnrollment)
                return Response<EnrollmentDto>.Fail("Already enrolled in this section", 400);

            // Create enrollment
            var enrollment = new Enrollment
            {
                StudentId = studentId,
                SectionId = dto.SectionId,
                Status = EnrollmentStatus.Enrolled,
                EnrollmentDate = DateTime.UtcNow
            };

            await _enrollmentDal.AddAsync(enrollment);
            
            // Increment enrolled count atomically
            var success = await _sectionDal.IncrementEnrolledCountAsync(dto.SectionId);
            if (!success)
                return Response<EnrollmentDto>.Fail("Failed to update section capacity", 500);

            await _context.SaveChangesAsync();

            // Map to DTO
            var resultDto = new EnrollmentDto
            {
                Id = enrollment.Id,
                Status = enrollment.Status,
                EnrollmentDate = enrollment.EnrollmentDate,
                StudentId = studentId,
                SectionId = dto.SectionId,
                CourseCode = section.Course.Code,
                CourseName = section.Course.Name,
                SectionNumber = section.SectionNumber,
                InstructorName = $"{section.Instructor.Title} {section.Instructor.User.FullName}"
            };

            return Response<EnrollmentDto>.Success(resultDto, 201);
        }

        public async Task<Response<NoDataDto>> DropCourseAsync(int studentId, int enrollmentId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Section)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.StudentId == studentId);

            if (enrollment == null)
                return Response<NoDataDto>.Fail("Enrollment not found", 404);

            // Check if within drop period (first 4 weeks)
            var daysSinceEnrollment = (DateTime.UtcNow - enrollment.EnrollmentDate).Days;
            if (daysSinceEnrollment > 28)
            {
                // After 4 weeks, mark as Withdrawn instead of Dropped
                enrollment.Status = EnrollmentStatus.Withdrawn;
            }
            else
            {
                enrollment.Status = EnrollmentStatus.Dropped;
                // Decrement enrolled count only for drops
                await _sectionDal.DecrementEnrolledCountAsync(enrollment.SectionId);
            }

            _enrollmentDal.Update(enrollment);
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<IEnumerable<StudentCourseDto>>> GetMyCoursesAsync(int studentId)
        {
            var enrollments = await _enrollmentDal.GetEnrollmentsByStudentAsync(studentId);

            var courses = enrollments
                .Where(e => e.Status == EnrollmentStatus.Enrolled)
                .Select(e => new StudentCourseDto
                {
                    EnrollmentId = e.Id,
                    CourseCode = e.Section.Course.Code,
                    CourseName = e.Section.Course.Name,
                    SectionNumber = e.Section.SectionNumber,
                    InstructorName = $"{e.Section.Instructor.Title} {e.Section.Instructor.User.FullName}",
                    Credits = e.Section.Course.Credits,
                    ScheduleJson = e.Section.ScheduleJson,
                    Status = e.Status,
                    MidtermGrade = e.MidtermGrade,
                    FinalGrade = e.FinalGrade,
                    LetterGrade = e.LetterGrade
                });

            return Response<IEnumerable<StudentCourseDto>>.Success(courses, 200);
        }

        public async Task<Response<IEnumerable<SectionStudentDto>>> GetStudentsBySectionAsync(int sectionId, int instructorId)
        {
            // Verify instructor owns this section
            var section = await _context.CourseSections
                .FirstOrDefaultAsync(s => s.Id == sectionId && s.InstructorId == instructorId);

            if (section == null)
                return Response<IEnumerable<SectionStudentDto>>.Fail("Section not found or access denied", 404);

            var enrollments = await _enrollmentDal.GetEnrollmentsBySectionAsync(sectionId);

            var students = enrollments
                .Where(e => e.Status == EnrollmentStatus.Enrolled)
                .Select(e => new SectionStudentDto
                {
                    StudentId = e.StudentId,
                    StudentNumber = e.Student.StudentNumber,
                    StudentName = e.Student.User.FullName,
                    Email = e.Student.User.Email ?? "",
                    EnrollmentDate = e.EnrollmentDate,
                    MidtermGrade = e.MidtermGrade,
                    FinalGrade = e.FinalGrade,
                    LetterGrade = e.LetterGrade
                });

            return Response<IEnumerable<SectionStudentDto>>.Success(students, 200);
        }

        public async Task<Response<NoDataDto>> CheckPrerequisitesAsync(int studentId, int courseId)
        {
            // Get all prerequisites recursively
            var prerequisiteIds = await _prerequisiteDal.GetAllPrerequisiteIdsRecursiveAsync(courseId);

            if (!prerequisiteIds.Any())
                return Response<NoDataDto>.Success(200);

            // Check which prerequisites the student has completed
            var completedCourseIds = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Completed)
                .Select(e => e.Section.CourseId)
                .ToListAsync();

            var missingPrerequisites = prerequisiteIds.Except(completedCourseIds).ToList();

            if (missingPrerequisites.Any())
            {
                var missingCourses = await _context.Courses
                    .Where(c => missingPrerequisites.Contains(c.Id))
                    .Select(c => $"{c.Code} - {c.Name}")
                    .ToListAsync();

                return Response<NoDataDto>.Fail(
                    $"Missing prerequisites: {string.Join(", ", missingCourses)}", 400);
            }

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> CheckScheduleConflictAsync(int studentId, int sectionId)
        {
            var newSection = await _context.CourseSections.FindAsync(sectionId);
            if (newSection == null || string.IsNullOrEmpty(newSection.ScheduleJson))
                return Response<NoDataDto>.Success(200);

            // Get student's current enrolled sections
            var currentSections = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Enrolled)
                .Select(e => e.Section)
                .ToListAsync();

            var newSchedule = ParseSchedule(newSection.ScheduleJson);

            foreach (var current in currentSections)
            {
                if (string.IsNullOrEmpty(current.ScheduleJson)) continue;

                var existingSchedule = ParseSchedule(current.ScheduleJson);

                if (HasTimeConflict(newSchedule, existingSchedule))
                {
                    return Response<NoDataDto>.Fail(
                        $"Schedule conflict with {current.Course?.Code ?? "another course"}", 400);
                }
            }

            return Response<NoDataDto>.Success(200);
        }

        private List<ScheduleEntry> ParseSchedule(string scheduleJson)
        {
            try
            {
                return JsonSerializer.Deserialize<List<ScheduleEntry>>(scheduleJson) ?? new List<ScheduleEntry>();
            }
            catch
            {
                return new List<ScheduleEntry>();
            }
        }

        private bool HasTimeConflict(List<ScheduleEntry> schedule1, List<ScheduleEntry> schedule2)
        {
            foreach (var s1 in schedule1)
            {
                foreach (var s2 in schedule2)
                {
                    if (s1.Day.Equals(s2.Day, StringComparison.OrdinalIgnoreCase))
                    {
                        var start1 = TimeSpan.Parse(s1.StartTime);
                        var end1 = TimeSpan.Parse(s1.EndTime);
                        var start2 = TimeSpan.Parse(s2.StartTime);
                        var end2 = TimeSpan.Parse(s2.EndTime);

                        // Check overlap
                        if (start1 < end2 && start2 < end1)
                            return true;
                    }
                }
            }
            return false;
        }

        private class ScheduleEntry
        {
            public string Day { get; set; } = "";
            public string StartTime { get; set; } = "";
            public string EndTime { get; set; } = "";
            public int? ClassroomId { get; set; }
        }
    }
}
