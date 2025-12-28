using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfFacultyDal : GenericRepository<Faculty>, IFacultyDal
    {
        public EfFacultyDal(CampusContext context) : base(context)
        {
        }

        public async Task<Faculty?> GetByUserIdAsync(string userId)
        {
            return await _context.Faculties.FirstOrDefaultAsync(f => f.UserId == userId);
        }

        public async Task<Faculty?> GetFacultyWithUserAsync(int facultyId)
        {
            return await _context.Faculties
                .Include(f => f.User)
                .Include(f => f.Department)
                .FirstOrDefaultAsync(f => f.Id == facultyId);
        }
    }
}
