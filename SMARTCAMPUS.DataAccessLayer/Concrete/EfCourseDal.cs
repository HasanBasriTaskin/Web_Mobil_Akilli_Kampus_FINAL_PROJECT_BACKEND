using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfCourseDal : GenericRepository<Course>, ICourseDal
    {
        public EfCourseDal(CampusContext context) : base(context)
        {
        }

        public async Task<Course?> GetCourseWithPrerequisitesAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteCourse)
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.Id == courseId);
        }

        public async Task<Course?> GetCourseByCodeAsync(string code)
        {
            return await _context.Courses
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.Code == code);
        }

        public async Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(int departmentId)
        {
            return await _context.Courses
                .Where(c => c.DepartmentId == departmentId && c.IsActive)
                .Include(c => c.Department)
                .ToListAsync();
        }

        public async Task<bool> CheckPrerequisiteAsync(int courseId, int studentId)
        {
            var course = await GetCourseWithPrerequisitesAsync(courseId);
            if (course == null || !course.Prerequisites.Any())
                return true;

            var studentEnrollments = await _context.Enrollments
                .Where(e => e.StudentId == studentId 
                    && (e.Status == "Completed" || e.LetterGrade != "F")
                    && e.IsActive)
                .Include(e => e.Section)
                    .ThenInclude(s => s.Course)
                .ToListAsync();

            var completedCourseIds = studentEnrollments
                .Select(e => e.Section.CourseId)
                .Distinct()
                .ToList();

            return course.Prerequisites
                .All(p => completedCourseIds.Contains(p.PrerequisiteCourseId));
        }
    }
}



