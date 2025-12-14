using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfClassroomDal : GenericRepository<Classroom>, IClassroomDal
    {
        public EfClassroomDal(CampusContext context) : base(context)
        {
        }

        public async Task<Classroom?> GetClassroomByBuildingAndRoomAsync(string building, string roomNumber)
        {
            return await _context.Classrooms
                .FirstOrDefaultAsync(c => c.Building == building && c.RoomNumber == roomNumber && c.IsActive);
        }

        public async Task<IEnumerable<Classroom>> GetAvailableClassroomsAsync(DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            // Get classrooms that don't have conflicting sections at the given time
            var conflictingSectionIds = await _context.CourseSections
                .Where(s => s.IsActive && s.ScheduleJson != null)
                .Select(s => s.Id)
                .ToListAsync();

            // This is a simplified check - in a real scenario, you'd parse ScheduleJson
            // and check for actual time conflicts
            return await _context.Classrooms
                .Where(c => c.IsActive)
                .ToListAsync();
        }
    }
}



