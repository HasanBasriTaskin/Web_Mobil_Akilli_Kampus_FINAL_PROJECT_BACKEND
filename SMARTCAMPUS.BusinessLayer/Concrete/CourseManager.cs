using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Course;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class CourseManager : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourseManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<IEnumerable<CourseListDto>>> GetAllCoursesAsync(int page, int pageSize, int? departmentId = null, string? search = null)
        {
            var courses = await _unitOfWork.Courses.GetAllCoursesWithDetailsAsync(page, pageSize, departmentId, search);

            var result = courses.Select(c => new CourseListDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Credits = c.Credits,
                ECTS = c.ECTS,
                DepartmentId = c.DepartmentId,
                DepartmentName = c.Department != null ? c.Department.Name : null,
                SectionCount = c.CourseSections.Count
            }).ToList();

            return Response<IEnumerable<CourseListDto>>.Success(result, 200);
        }

        public async Task<Response<CourseDto>> GetCourseByIdAsync(int id)
        {
            var course = await _unitOfWork.Courses.GetByIdWithDetailsAsync(id);

            if (course == null)
            {
                return Response<CourseDto>.Fail("Ders bulunamadı", 404);
            }

            var sections = new List<CourseSectionDto>();
            foreach (var s in course.CourseSections)
            {
                // Fetch schedules for this section from DAL
                var schedules = await _unitOfWork.Schedules.GetBySectionIdAsync(s.Id);
                var scheduleData = schedules.Select(sch => new
                {
                    Day = sch.DayOfWeek.ToString(),
                    Start = sch.StartTime.ToString(@"hh\:mm"),
                    End = sch.EndTime.ToString(@"hh\:mm"),
                    Classroom = sch.Classroom != null ? $"{sch.Classroom.Building}-{sch.Classroom.RoomNumber}" : null
                });
                var scheduleJson = schedules.Any() 
                    ? System.Text.Json.JsonSerializer.Serialize(scheduleData)
                    : null;

                sections.Add(new CourseSectionDto
                {
                    Id = s.Id,
                    SectionNumber = s.SectionNumber,
                    Semester = s.Semester,
                    Year = s.Year,
                    Capacity = s.Capacity,
                    EnrolledCount = s.EnrolledCount,
                    ScheduleJson = scheduleJson,
                    CourseId = s.CourseId,
                    CourseCode = course.Code,
                    CourseName = course.Name,
                    InstructorId = s.InstructorId,
                    InstructorName = s.Instructor?.User?.FullName ?? "TBA",
                    InstructorTitle = s.Instructor?.Title ?? ""
                });
            }

            var dto = new CourseDto
            {
                Id = course.Id,
                Code = course.Code,
                Name = course.Name,
                Description = course.Description,
                Credits = course.Credits,
                ECTS = course.ECTS,
                DepartmentId = course.DepartmentId,
                DepartmentName = course.Department?.Name,
                Sections = sections
            };

            return Response<CourseDto>.Success(dto, 200);
        }

        public async Task<Response<CourseDto>> CreateCourseAsync(CreateCourseDto dto)
        {
            var existingCourse = await _unitOfWork.Courses.Where(c => c.Code == dto.Code).FirstOrDefaultAsync();

            if (existingCourse != null)
            {
                return Response<CourseDto>.Fail("Bu ders kodu zaten kullanılıyor", 400);
            }

            var course = new Course
            {
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                Credits = dto.Credits,
                ECTS = dto.ECTS,
                DepartmentId = dto.DepartmentId
            };

            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.CommitAsync();

            return Response<CourseDto>.Success(new CourseDto
            {
                Id = course.Id,
                Code = course.Code,
                Name = course.Name,
                Description = course.Description,
                Credits = course.Credits,
                ECTS = course.ECTS,
                DepartmentId = course.DepartmentId
            }, 201);
        }

        public async Task<Response<CourseDto>> UpdateCourseAsync(int id, UpdateCourseDto dto)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null)
            {
                return Response<CourseDto>.Fail("Ders bulunamadı", 404);
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                course.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Description))
                course.Description = dto.Description;
            if (dto.Credits.HasValue)
                course.Credits = dto.Credits.Value;
            if (dto.ECTS.HasValue)
                course.ECTS = dto.ECTS.Value;

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.CommitAsync();

            return Response<CourseDto>.Success(new CourseDto
            {
                Id = course.Id,
                Code = course.Code,
                Name = course.Name,
                Description = course.Description,
                Credits = course.Credits,
                ECTS = course.ECTS,
                DepartmentId = course.DepartmentId
            }, 200);
        }

        public async Task<Response<NoDataDto>> DeleteCourseAsync(int id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null)
            {
                return Response<NoDataDto>.Fail("Ders bulunamadı", 404);
            }

            // Note: EF Core Generic Repository usually has Remove(T entity)
            _unitOfWork.Courses.Remove(course);
            await _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Success(204);
        }

        public async Task<Response<IEnumerable<CoursePrerequisiteDto>>> GetPrerequisitesAsync(int courseId)
        {
            var course = await _unitOfWork.Courses.GetCourseWithPrerequisitesAsync(courseId);
            if (course == null)
            {
                return Response<IEnumerable<CoursePrerequisiteDto>>.Fail("Ders bulunamadı", 404);
            }

            // Note: GetCourseWithPrerequisitesAsync already includes prerequisites
            var result = course.Prerequisites.Select(p => new CoursePrerequisiteDto
            {
                CourseId = p.PrerequisiteCourseId,
                CourseCode = p.PrerequisiteCourse != null ? p.PrerequisiteCourse.Code : "",
                CourseName = p.PrerequisiteCourse != null ? p.PrerequisiteCourse.Name : ""
            }).ToList();

            return Response<IEnumerable<CoursePrerequisiteDto>>.Success(result, 200);
        }
    }
}
