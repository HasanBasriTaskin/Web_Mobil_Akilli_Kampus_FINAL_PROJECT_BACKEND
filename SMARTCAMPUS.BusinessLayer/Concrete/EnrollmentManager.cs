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

            // Check if there is an existing enrollment for THIS section
            var existingSameSection = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.SectionId == dto.SectionId);

            if (existingSameSection != null)
            {
                // If currently active
                if (existingSameSection.Status == EnrollmentStatus.Pending || existingSameSection.Status == EnrollmentStatus.Enrolled)
                {
                    return Response<EnrollmentDto>.Fail("Bu derse zaten kayıtlısınız veya onay bekliyor", 400);
                }

                // If previously rejected/dropped/withdrawn, Reactivate it as Pending
                existingSameSection.Status = EnrollmentStatus.Pending;
                existingSameSection.EnrollmentDate = DateTime.UtcNow;
                existingSameSection.MidtermGrade = null;
                existingSameSection.FinalGrade = null;
                existingSameSection.LetterGrade = null;
                existingSameSection.GradePoint = null;

                _enrollmentDal.Update(existingSameSection);
                await _context.SaveChangesAsync();

                // Build DTO
                var resultDto = new EnrollmentDto
                {
                    Id = existingSameSection.Id,
                    Status = existingSameSection.Status,
                    EnrollmentDate = existingSameSection.EnrollmentDate,
                    StudentId = studentId,
                    SectionId = dto.SectionId,
                    CourseCode = section.Course.Code,
                    CourseName = section.Course.Name,
                    SectionNumber = section.SectionNumber,
                    InstructorName = $"{section.Instructor.Title} {section.Instructor.User.FullName}"
                };
                return Response<EnrollmentDto>.Success(resultDto, 200); // 200 OK for update
            }

            // Check if enrolled in ANOTHER section of the SAME course
            var existingOtherSection = await _context.Enrollments
                .Include(e => e.Section)
                .AnyAsync(e => e.StudentId == studentId 
                    && e.Section.CourseId == section.CourseId
                    && (e.Status == EnrollmentStatus.Pending || e.Status == EnrollmentStatus.Enrolled));
            
            if (existingOtherSection)
                return Response<EnrollmentDto>.Fail("Bu dersin başka bir seksiyonuna zaten kayıtlısınız", 400);

            // Create new enrollment
            var enrollment = new Enrollment
            {
                StudentId = studentId,
                SectionId = dto.SectionId,
                Status = EnrollmentStatus.Pending,
                EnrollmentDate = DateTime.UtcNow
            };

            await _enrollmentDal.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Map to DTO
            var newResultDto = new EnrollmentDto
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

            return Response<EnrollmentDto>.Success(newResultDto, 201);
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
                    ScheduleJson = null, // TODO:  Schedule entity'den alınacak
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
                    EnrollmentId = e.Id,
                    StudentNumber = e.Student.StudentNumber,
                    StudentName = e.Student.User.FullName,
                    Email = e.Student.User.Email ?? "",
                    EnrollmentDate = e.EnrollmentDate,
                    Status = e.Status,
                    MidtermGrade = e.MidtermGrade,
                    FinalGrade = e.FinalGrade,
                    LetterGrade = e.LetterGrade
                });

            return Response<IEnumerable<SectionStudentDto>>.Success(students, 200);
        }

        public async Task<Response<IEnumerable<FacultySectionDto>>> GetMySectionsAsync(int instructorId)
        {
            var sections = await _context.CourseSections
                .Include(s => s.Course)
                .Where(s => s.InstructorId == instructorId)
                .Select(s => new FacultySectionDto
                {
                    Id = s.Id,
                    CourseId = s.CourseId,
                    CourseCode = s.Course.Code,
                    CourseName = s.Course.Name,
                    SectionNumber = s.SectionNumber,
                    Semester = s.Semester,
                    Year = s.Year,
                    Capacity = s.Capacity,
                    EnrolledCount = s.EnrolledCount,
                    PendingCount = _context.Enrollments.Count(e => e.SectionId == s.Id && e.Status == EnrollmentStatus.Pending)
                })
                .ToListAsync();

            return Response<IEnumerable<FacultySectionDto>>.Success(sections, 200);
        }

        public async Task<Response<IEnumerable<PendingEnrollmentDto>>> GetPendingEnrollmentsAsync(int sectionId, int instructorId)
        {
            // Verify instructor owns this section
            var section = await _context.CourseSections
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == sectionId && s.InstructorId == instructorId);

            if (section == null)
                return Response<IEnumerable<PendingEnrollmentDto>>.Fail("Section not found or access denied", 404);

            var pendingEnrollments = await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Section)
                    .ThenInclude(sec => sec.Course)
                .Where(e => e.SectionId == sectionId && e.Status == EnrollmentStatus.Pending)
                .Select(e => new PendingEnrollmentDto
                {
                    EnrollmentId = e.Id,
                    StudentId = e.StudentId,
                    StudentNumber = e.Student.StudentNumber,
                    StudentName = e.Student.User.FullName,
                    Email = e.Student.User.Email ?? "",
                    RequestDate = e.EnrollmentDate,
                    SectionId = e.SectionId,
                    CourseCode = e.Section.Course.Code,
                    CourseName = e.Section.Course.Name,
                    SectionNumber = e.Section.SectionNumber
                })
                .ToListAsync();

            return Response<IEnumerable<PendingEnrollmentDto>>.Success(pendingEnrollments, 200);
        }

        public async Task<Response<NoDataDto>> ApproveEnrollmentAsync(int enrollmentId, int instructorId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Section)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
                return Response<NoDataDto>.Fail("Enrollment not found", 404);

            // Verify instructor owns the section
            if (enrollment.Section.InstructorId != instructorId)
                return Response<NoDataDto>.Fail("Access denied - not your section", 403);

            if (enrollment.Status != EnrollmentStatus.Pending)
                return Response<NoDataDto>.Fail("This enrollment is not pending", 400);

            // Check capacity before approval
            if (enrollment.Section.EnrolledCount >= enrollment.Section.Capacity)
                return Response<NoDataDto>.Fail("Section is now full, cannot approve", 400);

            // Approve the enrollment
            enrollment.Status = EnrollmentStatus.Enrolled;
            
            // Increment enrolled count
            await _sectionDal.IncrementEnrolledCountAsync(enrollment.SectionId);
            
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> RejectEnrollmentAsync(int enrollmentId, int instructorId, string? reason)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Section)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
                return Response<NoDataDto>.Fail("Enrollment not found", 404);

            // Verify instructor owns the section
            if (enrollment.Section.InstructorId != instructorId)
                return Response<NoDataDto>.Fail("Access denied - not your section", 403);

            if (enrollment.Status != EnrollmentStatus.Pending)
                return Response<NoDataDto>.Fail("This enrollment is not pending", 400);

            // Reject the enrollment
            enrollment.Status = EnrollmentStatus.Rejected;
            // Note: reason could be stored in a new field if needed
            
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
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
            // Schedule entity üzerinden çakışma kontrolü
            var newSchedules = await _context.Schedules
                .Where(s => s.SectionId == sectionId)
                .ToListAsync();
            
            if (!newSchedules.Any())
                return Response<NoDataDto>.Success(200); // No schedules defined yet
            
            // Get student's current enrolled sections' schedules
            var enrolledSectionIds = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Enrolled)
                .Select(e => e.SectionId)
                .ToListAsync();
            
            var existingSchedules = await _context.Schedules
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Where(s => enrolledSectionIds.Contains(s.SectionId))
                .ToListAsync();
            
            foreach (var newSchedule in newSchedules)
            {
                foreach (var existing in existingSchedules)
                {
                    if (newSchedule.DayOfWeek == existing.DayOfWeek)
                    {
                        // Check time overlap
                        if (newSchedule.StartTime < existing.EndTime && existing.StartTime < newSchedule.EndTime)
                        {
                            return Response<NoDataDto>.Fail(
                                $"Schedule conflict with {existing.Section?.Course?.Code ?? "another course"}", 400);
                        }
                    }
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
                    if (string.IsNullOrWhiteSpace(s1.StartTime) || string.IsNullOrWhiteSpace(s1.EndTime) ||
                        string.IsNullOrWhiteSpace(s2.StartTime) || string.IsNullOrWhiteSpace(s2.EndTime))
                    {
                        continue;
                    }

                    if (s1.Day.Equals(s2.Day, StringComparison.OrdinalIgnoreCase))
                    {
                        TimeSpan start1, end1, start2, end2;
                        
                        try 
                        {
                            start1 = TimeSpan.Parse(s1.StartTime);
                            end1 = TimeSpan.Parse(s1.EndTime);
                            start2 = TimeSpan.Parse(s2.StartTime);
                            end2 = TimeSpan.Parse(s2.EndTime);
                        }
                        catch
                        {
                            continue; // Skip invalid formats
                        }

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
