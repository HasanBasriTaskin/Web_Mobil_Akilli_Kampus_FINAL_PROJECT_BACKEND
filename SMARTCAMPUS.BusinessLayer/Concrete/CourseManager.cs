using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Course;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class CourseManager : ICourseService
    {
        private readonly ICourseDal _courseDal;
        private readonly ICoursePrerequisiteDal _prerequisiteDal;
        private readonly CampusContext _context;

        public CourseManager(ICourseDal courseDal, ICoursePrerequisiteDal prerequisiteDal, CampusContext context)
        {
            _courseDal = courseDal;
            _prerequisiteDal = prerequisiteDal;
            _context = context;
        }

        public async Task<Response<IEnumerable<CourseListDto>>> GetAllCoursesAsync(int page, int pageSize, int? departmentId = null, string? search = null)
        {
            var query = _context.Courses
                .Include(c => c.Department)
                .Include(c => c.CourseSections)
                .AsQueryable();

            if (departmentId.HasValue)
            {
                query = query.Where(c => c.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(c => 
                    c.Code.ToLower().Contains(searchLower) || 
                    c.Name.ToLower().Contains(searchLower));
            }

            var courses = await query
                .OrderBy(c => c.Code)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CourseListDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Credits = c.Credits,
                    ECTS = c.ECTS,
                    DepartmentId = c.DepartmentId,
                    DepartmentName = c.Department != null ? c.Department.Name : null,
                    SectionCount = c.CourseSections.Count
                })
                .ToListAsync();

            return Response<IEnumerable<CourseListDto>>.Success(courses, 200);
        }

        public async Task<Response<CourseDto>> GetCourseByIdAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.CourseSections)
                    .ThenInclude(s => s.Instructor)
                        .ThenInclude(f => f!.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return Response<CourseDto>.Fail("Ders bulunamadı", 404);
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
                Sections = course.CourseSections.Select(s => new CourseSectionDto
                {
                    Id = s.Id,
                    SectionNumber = s.SectionNumber,
                    Semester = s.Semester,
                    Year = s.Year,
                    Capacity = s.Capacity,
                    EnrolledCount = s.EnrolledCount,
                    ScheduleJson = s.ScheduleJson,
                    CourseId = s.CourseId,
                    CourseCode = course.Code,
                    CourseName = course.Name,
                    InstructorId = s.InstructorId,
                    InstructorName = s.Instructor?.User?.FullName ?? "TBA",
                    InstructorTitle = s.Instructor?.Title ?? ""
                }).ToList()
            };

            return Response<CourseDto>.Success(dto, 200);
        }

        public async Task<Response<CourseDto>> CreateCourseAsync(CreateCourseDto dto)
        {
            var existingCourse = await _context.Courses
                .FirstOrDefaultAsync(c => c.Code == dto.Code);

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

            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

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
            var course = await _context.Courses.FindAsync(id);
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

            await _context.SaveChangesAsync();

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
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return Response<NoDataDto>.Fail("Ders bulunamadı", 404);
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return Response<NoDataDto>.Success(204);
        }

        public async Task<Response<IEnumerable<CoursePrerequisiteDto>>> GetPrerequisitesAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return Response<IEnumerable<CoursePrerequisiteDto>>.Fail("Ders bulunamadı", 404);
            }

            var prerequisites = await _context.CoursePrerequisites
                .Where(p => p.CourseId == courseId)
                .Include(p => p.PrerequisiteCourse)
                .Select(p => new CoursePrerequisiteDto
                {
                    CourseId = p.PrerequisiteCourseId,
                    CourseCode = p.PrerequisiteCourse != null ? p.PrerequisiteCourse.Code : "",
                    CourseName = p.PrerequisiteCourse != null ? p.PrerequisiteCourse.Name : ""
                })
                .ToListAsync();

            return Response<IEnumerable<CoursePrerequisiteDto>>.Success(prerequisites, 200);
        }
    }
}
