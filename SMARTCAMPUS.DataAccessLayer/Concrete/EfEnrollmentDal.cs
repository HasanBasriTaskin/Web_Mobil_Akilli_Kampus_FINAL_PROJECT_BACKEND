using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfEnrollmentDal : GenericRepository<Enrollment>, IEnrollmentDal
    {
        public EfEnrollmentDal(CampusContext context) : base(context)
        {
        }

        public async Task<Enrollment?> GetEnrollmentWithDetailsAsync(int enrollmentId)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Student)
                    .ThenInclude(s => s.Department)
                .Include(e => e.Section)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(int studentId)
        {
            return await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.IsActive)
                .Include(e => e.Section)
                    .ThenInclude(s => s.Course)
                .Include(e => e.Section)
                    .ThenInclude(s => s.Instructor)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsBySectionAsync(int sectionId)
        {
            return await _context.Enrollments
                .Where(e => e.SectionId == sectionId && e.IsActive)
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .OrderBy(e => e.Student.StudentNumber)
                .ToListAsync();
        }

        public async Task<Enrollment?> GetEnrollmentByStudentAndSectionAsync(int studentId, int sectionId)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Section)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.SectionId == sectionId && e.IsActive);
        }

        public async Task<bool> IsEnrolledAsync(int studentId, int sectionId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId 
                    && e.SectionId == sectionId 
                    && e.Status == "Active" 
                    && e.IsActive);
        }
    }
}



