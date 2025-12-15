using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfCoursePrerequisiteDal : ICoursePrerequisiteDal
    {
        private readonly CampusContext _context;

        public EfCoursePrerequisiteDal(CampusContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CoursePrerequisite>> GetPrerequisitesForCourseAsync(int courseId)
        {
            return await _context.CoursePrerequisites
                .Where(p => p.CourseId == courseId)
                .Include(p => p.PrerequisiteCourse)
                .ToListAsync();
        }

        public async Task<IEnumerable<int>> GetAllPrerequisiteIdsRecursiveAsync(int courseId)
        {
            var allPrerequisites = new HashSet<int>();
            await GetPrerequisitesRecursiveHelper(courseId, allPrerequisites);
            return allPrerequisites;
        }

        private async Task GetPrerequisitesRecursiveHelper(int courseId, HashSet<int> visited)
        {
            var prerequisites = await _context.CoursePrerequisites
                .Where(p => p.CourseId == courseId)
                .Select(p => p.PrerequisiteCourseId)
                .ToListAsync();

            foreach (var prereqId in prerequisites)
            {
                if (!visited.Contains(prereqId))
                {
                    visited.Add(prereqId);
                    await GetPrerequisitesRecursiveHelper(prereqId, visited);
                }
            }
        }

        public async Task AddAsync(CoursePrerequisite entity)
        {
            await _context.CoursePrerequisites.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(int courseId, int prerequisiteCourseId)
        {
            var entity = await _context.CoursePrerequisites
                .FirstOrDefaultAsync(p => p.CourseId == courseId && p.PrerequisiteCourseId == prerequisiteCourseId);
            
            if (entity != null)
            {
                _context.CoursePrerequisites.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int courseId, int prerequisiteCourseId)
        {
            return await _context.CoursePrerequisites
                .AnyAsync(p => p.CourseId == courseId && p.PrerequisiteCourseId == prerequisiteCourseId);
        }
    }
}
