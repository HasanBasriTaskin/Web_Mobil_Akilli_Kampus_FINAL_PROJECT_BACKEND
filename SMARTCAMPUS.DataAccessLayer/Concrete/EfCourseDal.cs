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
                    && (e.Status == SMARTCAMPUS.EntityLayer.Constants.EnrollmentStatus.Completed || e.LetterGrade != "F")
                    && e.IsActive)
                .Include(e => e.Section)
                    .ThenInclude(s => s.Course)
                .ToListAsync();

            var completedCourseIds = studentEnrollments
                .Select(e => e.Section.CourseId)
                .Distinct()
                .ToList();

            // Recursive prerequisite checking
            var visited = new HashSet<int>();
            return await CheckPrerequisitesRecursiveAsync(courseId, completedCourseIds, visited);
        }

        private async Task<bool> CheckPrerequisitesRecursiveAsync(int courseId, List<int> completedCourseIds, HashSet<int> visited)
        {
            // Prevent infinite loops
            if (visited.Contains(courseId))
                return true;

            visited.Add(courseId);

            var course = await GetCourseWithPrerequisitesAsync(courseId);
            if (course == null || !course.Prerequisites.Any())
                return true;

            foreach (var prereq in course.Prerequisites)
            {
                // Check if prerequisite is completed
                if (!completedCourseIds.Contains(prereq.PrerequisiteCourseId))
                    return false;

                // Recursive check for prerequisite's prerequisites
                if (!await CheckPrerequisitesRecursiveAsync(prereq.PrerequisiteCourseId, completedCourseIds, visited))
                    return false;
            }

            return true;
        }
    }
}



