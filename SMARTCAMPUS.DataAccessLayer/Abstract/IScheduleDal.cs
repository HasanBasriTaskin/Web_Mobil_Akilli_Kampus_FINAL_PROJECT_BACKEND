using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IScheduleDal : IGenericDal<Schedule>
    {
        Task<Schedule?> GetByIdWithDetailsAsync(int id);
        Task<List<Schedule>> GetBySectionIdAsync(int sectionId);
        Task<List<Schedule>> GetByClassroomIdAsync(int classroomId, DayOfWeek? dayOfWeek = null);
        Task<List<Schedule>> GetByInstructorIdAsync(int facultyId, DayOfWeek? dayOfWeek = null);
        Task<List<Schedule>> GetConflictsAsync(int classroomId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
        Task<bool> HasConflictAsync(int classroomId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
        Task<Schedule?> GetConflictingScheduleAsync(int classroomId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
        Task<Schedule?> GetSectionConflictAsync(int sectionId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
        Task<List<Schedule>> GetSchedulesBySectionIdsAsync(List<int> sectionIds);
    }
}
