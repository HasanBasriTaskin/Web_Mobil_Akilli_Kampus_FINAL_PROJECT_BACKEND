using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IClassroomReservationDal : IGenericDal<ClassroomReservation>
    {
        Task<ClassroomReservation?> GetByIdWithDetailsAsync(int id);
        Task<List<ClassroomReservation>> GetByUserIdAsync(string userId);
        Task<List<ClassroomReservation>> GetByDateAsync(DateTime date, int? classroomId = null);
        Task<List<ClassroomReservation>> GetPendingAsync();

        Task<List<ClassroomReservation>> GetConflictsAsync(int classroomId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
        Task<bool> HasConflictAsync(int classroomId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
    }
}
