using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfClassroomDal : GenericRepository<Classroom>, IClassroomDal
    {
        private readonly CampusContext _context;

        public EfClassroomDal(CampusContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Classroom?> GetByBuildingAndRoomAsync(string building, string roomNumber)
        {
            return await _context.Classrooms
                .FirstOrDefaultAsync(c => c.Building == building && c.RoomNumber == roomNumber);
        }
    }
}
