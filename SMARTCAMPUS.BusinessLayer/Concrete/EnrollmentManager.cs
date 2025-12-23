using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Enrollment;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using System.Text.Json;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class EnrollmentManager : IEnrollmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EnrollmentManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<EnrollmentDto>> EnrollInCourseAsync(int studentId, CreateEnrollmentDto dto)
        {
            // Get section with course info
            var section = await _unitOfWork.CourseSections.GetSectionWithDetailsAsync(dto.SectionId);
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
            var existingSameSection = await _unitOfWork.Enrollments.GetByStudentAndSectionAsync(studentId, dto.SectionId);

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

                _unitOfWork.Enrollments.Update(existingSameSection);
                await _unitOfWork.CommitAsync();

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
            var existingOtherSection = await _unitOfWork.Enrollments.IsEnrolledInOtherSectionAsync(studentId, section.CourseId);
            
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

            await _unitOfWork.Enrollments.AddAsync(enrollment);
            await _unitOfWork.CommitAsync();

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
            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(enrollmentId);

            if (enrollment == null || enrollment.StudentId != studentId)
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
                await _unitOfWork.CourseSections.DecrementEnrolledCountAsync(enrollment.SectionId);
            }

            _unitOfWork.Enrollments.Update(enrollment);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<IEnumerable<StudentCourseDto>>> GetMyCoursesAsync(int studentId)
        {
            var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsByStudentAsync(studentId);

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
                    ScheduleJson = null,
                    Status = e.Status,
                    MidtermGrade = e.MidtermGrade,
                    FinalGrade = e.FinalGrade,
                    LetterGrade = e.LetterGrade
                }).ToList();

            return Response<IEnumerable<StudentCourseDto>>.Success(courses, 200);
        }

        public async Task<Response<IEnumerable<SectionStudentDto>>> GetStudentsBySectionAsync(int sectionId, int instructorId)
        {
            // Verify instructor owns this section
            var section = await _unitOfWork.CourseSections.GetByIdAsync(sectionId);

            if (section == null || section.InstructorId != instructorId)
                return Response<IEnumerable<SectionStudentDto>>.Fail("Section not found or access denied", 404);

            var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsBySectionAsync(sectionId);

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
                }).ToList();

            return Response<IEnumerable<SectionStudentDto>>.Success(students, 200);
        }

        public async Task<Response<IEnumerable<FacultySectionDto>>> GetMySectionsAsync(int instructorId)
        {
            var sections = await _unitOfWork.CourseSections.GetSectionsByInstructorAsync(instructorId);

            var result = new List<FacultySectionDto>();
            foreach(var s in sections)
            {
                 var pendingCount = await _unitOfWork.Enrollments.GetPendingEnrollmentsAsync(s.Id);
                 
                 result.Add(new FacultySectionDto
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
                    PendingCount = pendingCount.Count
                 });
            }

            return Response<IEnumerable<FacultySectionDto>>.Success(result, 200);
        }

        public async Task<Response<IEnumerable<PendingEnrollmentDto>>> GetPendingEnrollmentsAsync(int sectionId, int instructorId)
        {
            // Verify instructor owns this section
            var section = await _unitOfWork.CourseSections.GetSectionWithDetailsAsync(sectionId);

            if (section == null || section.InstructorId != instructorId)
                return Response<IEnumerable<PendingEnrollmentDto>>.Fail("Section not found or access denied", 404);

            var pendingEnrollments = await _unitOfWork.Enrollments.GetPendingEnrollmentsAsync(sectionId);

            var result = pendingEnrollments.Select(e => new PendingEnrollmentDto
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
            }).ToList();

            return Response<IEnumerable<PendingEnrollmentDto>>.Success(result, 200);
        }

        public async Task<Response<NoDataDto>> ApproveEnrollmentAsync(int enrollmentId, int instructorId)
        {
            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(enrollmentId);

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
            await _unitOfWork.CourseSections.IncrementEnrolledCountAsync(enrollment.SectionId);
            
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> RejectEnrollmentAsync(int enrollmentId, int instructorId, string? reason)
        {
             var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(enrollmentId);

            if (enrollment == null)
                return Response<NoDataDto>.Fail("Enrollment not found", 404);

            // Verify instructor owns the section
            if (enrollment.Section.InstructorId != instructorId)
                return Response<NoDataDto>.Fail("Access denied - not your section", 403);

            if (enrollment.Status != EnrollmentStatus.Pending)
                return Response<NoDataDto>.Fail("This enrollment is not pending", 400);

            // Reject the enrollment
            enrollment.Status = EnrollmentStatus.Rejected;
            
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> CheckPrerequisitesAsync(int studentId, int courseId)
        {
            var prerequisiteIds = await _unitOfWork.CoursePrerequisites.GetAllPrerequisiteIdsRecursiveAsync(courseId);

            if (!prerequisiteIds.Any())
                return Response<NoDataDto>.Success(200);

            // Check which prerequisites the student has completed
            var completedCourseIds = await _unitOfWork.Enrollments.GetCompletedCourseIdsAsync(studentId);

            var missingPrerequisites = prerequisiteIds.Except(completedCourseIds).ToList();

            if (missingPrerequisites.Any())
            {
                var missingNames = new List<string>();
                foreach(var id in missingPrerequisites)
                {
                    var c = await _unitOfWork.Courses.GetByIdAsync(id);
                    if(c != null) missingNames.Add($"{c.Code} - {c.Name}");
                }

                return Response<NoDataDto>.Fail(
                    $"Missing prerequisites: {string.Join(", ", missingNames)}", 400);
            }

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> CheckScheduleConflictAsync(int studentId, int sectionId)
        {
            // Get schedules for the new section
            var newSchedules = await _unitOfWork.Schedules.GetBySectionIdAsync(sectionId);
            
            if (!newSchedules.Any())
                return Response<NoDataDto>.Success(200);
            
            // Get student's current enrolled section IDs
            var enrolledSectionIds = await _unitOfWork.Enrollments.GetEnrolledSectionIdsAsync(studentId);
            
            if (!enrolledSectionIds.Any())
                return Response<NoDataDto>.Success(200);

            // Get schedules for these sections
            var existingSchedules = await _unitOfWork.Schedules.GetSchedulesBySectionIdsAsync(enrolledSectionIds);
            
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
    }
}
