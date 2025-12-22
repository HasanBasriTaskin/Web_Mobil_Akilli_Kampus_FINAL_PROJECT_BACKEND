using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IScheduleService
    {
        // Listeleme
        Task<Response<List<ScheduleDto>>> GetSchedulesBySectionAsync(int sectionId);
        Task<Response<List<WeeklyScheduleDto>>> GetWeeklyScheduleAsync(int sectionId);
        Task<Response<List<ScheduleDto>>> GetSchedulesByClassroomAsync(int classroomId, DayOfWeek? dayOfWeek = null);
        Task<Response<List<ScheduleDto>>> GetSchedulesByInstructorAsync(int facultyId, DayOfWeek? dayOfWeek = null);
        
        // CRUD (Admin)
        Task<Response<ScheduleDto>> CreateScheduleAsync(ScheduleCreateDto dto);
        Task<Response<ScheduleDto>> UpdateScheduleAsync(int id, ScheduleUpdateDto dto);
        Task<Response<NoDataDto>> DeleteScheduleAsync(int id);
        
        // Çakışma kontrolü
        Task<Response<List<ScheduleConflictDto>>> CheckConflictsAsync(ScheduleCreateDto dto, int? excludeId = null);
    }
}
