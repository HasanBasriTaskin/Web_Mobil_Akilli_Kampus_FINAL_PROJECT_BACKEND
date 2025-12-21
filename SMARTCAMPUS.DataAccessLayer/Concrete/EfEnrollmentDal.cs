using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfEnrollmentDal : GenericRepository<Enrollment>, IEnrollmentDal
    {
        private readonly CampusContext _context;

        public EfEnrollmentDal(CampusContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(int studentId)
        {
            return await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Section)
                    .ThenInclude(s => s.Course)
                .Include(e => e.Section)
                    .ThenInclude(s => s.Instructor)
                        .ThenInclude(i => i.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsBySectionAsync(int sectionId)
        {
            return await _context.Enrollments
                .Where(e => e.SectionId == sectionId)
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .ToListAsync();
        }

        public async Task<Enrollment?> GetEnrollmentWithDetailsAsync(int id)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Section)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<bool> HasStudentCompletedCourseAsync(int studentId, int courseId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId 
                    && e.Section.CourseId == courseId 
                    && e.Status == EnrollmentStatus.Completed);
        }
    }
}
