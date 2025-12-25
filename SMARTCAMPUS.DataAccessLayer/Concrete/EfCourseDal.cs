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

        public async Task<Course?> GetCourseWithPrerequisitesAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteCourse)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(int departmentId)
        {
            return await _context.Courses
                .Where(c => c.DepartmentId == departmentId && c.IsActive)
                .Include(c => c.Department)
                .ToListAsync();
        }

        public async Task<List<Course>> GetAllCoursesWithDetailsAsync(int page, int pageSize, int? departmentId = null, string? search = null)
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

            return await query
                .OrderBy(c => c.Code)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Course?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.CourseSections)
                    .ThenInclude(s => s.Instructor)
                        .ThenInclude(f => f!.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
