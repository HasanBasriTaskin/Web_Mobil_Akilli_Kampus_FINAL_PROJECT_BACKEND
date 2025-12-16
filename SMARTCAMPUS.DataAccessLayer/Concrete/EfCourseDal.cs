using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfCourseDal : GenericRepository<Course>, ICourseDal
    {
        private readonly CampusContext _context;

        public EfCourseDal(CampusContext context) : base(context)
        {
            _context = context;
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
    }
}
