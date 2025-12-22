using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfCourseSectionDal : GenericRepository<CourseSection>, ICourseSectionDal
    {
        public EfCourseSectionDal(CampusContext context) : base(context)
        {
        }

        public async Task<CourseSection?> GetSectionWithDetailsAsync(int id)
        {
            return await _context.CourseSections
                .Include(s => s.Course)
                .Include(s => s.Instructor)
                    .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<CourseSection>> GetSectionsByInstructorAsync(int instructorId)
        {
            return await _context.CourseSections
                .Where(s => s.InstructorId == instructorId)
                .Include(s => s.Course)
                .ToListAsync();
        }

        public async Task IncrementEnrolledCountAsync(int sectionId)
        {
            // Atomic update with capacity check to prevent over-enrollment
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE CourseSections SET EnrolledCount = EnrolledCount + 1 WHERE Id = {0} AND EnrolledCount < Capacity",
                sectionId);
        }

        public async Task DecrementEnrolledCountAsync(int sectionId)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE CourseSections SET EnrolledCount = EnrolledCount - 1 WHERE Id = {0} AND EnrolledCount > 0",
                sectionId);
        }
    }
}
