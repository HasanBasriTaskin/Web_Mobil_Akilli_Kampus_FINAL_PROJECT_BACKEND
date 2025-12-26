using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfStudentDal : GenericRepository<Student>, IStudentDal
    {
        public EfStudentDal(CampusContext context) : base(context)
        {
        }

        public async Task<Student?> GetStudentWithDetailsAsync(int id)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
        }

        public async Task<Student?> GetByUserIdAsync(string userId)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
        }
    }
}

