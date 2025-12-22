using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class ScheduleManager : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ScheduleManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<List<ScheduleDto>>> GetSchedulesBySectionAsync(int sectionId)
        {
            var schedules = await _unitOfWork.Schedules.GetBySectionIdAsync(sectionId);
            var result = schedules.Select(s => MapToDto(s)).ToList();
            return Response<List<ScheduleDto>>.Success(result, 200);
        }

        public async Task<Response<List<WeeklyScheduleDto>>> GetWeeklyScheduleAsync(int sectionId)
        {
            var section = await _unitOfWork.CourseSections.GetByIdAsync(sectionId);
            if (section == null)
                return Response<List<WeeklyScheduleDto>>.Fail("Ders bölümü bulunamadı", 404);

            var schedules = await _unitOfWork.Schedules.GetBySectionIdAsync(sectionId);

            var result = schedules
                .GroupBy(s => s.DayOfWeek)
                .Select(g => new WeeklyScheduleDto
                {
                    Day = g.Key,
                    Schedules = g.OrderBy(x => x.StartTime).Select(s => MapToDto(s)).ToList()
                })
                .ToList();

            return Response<List<WeeklyScheduleDto>>.Success(result, 200);
        }

        public async Task<Response<List<ScheduleDto>>> GetSchedulesByClassroomAsync(int classroomId, DayOfWeek? dayOfWeek = null)
        {
            var schedules = await _unitOfWork.Schedules.GetByClassroomIdAsync(classroomId, dayOfWeek);
            var result = schedules.Select(s => MapToDto(s)).ToList();
            return Response<List<ScheduleDto>>.Success(result, 200);
        }

        public async Task<Response<List<ScheduleDto>>> GetSchedulesByInstructorAsync(int facultyId, DayOfWeek? dayOfWeek = null)
        {
            var schedules = await _unitOfWork.Schedules.GetByInstructorIdAsync(facultyId, dayOfWeek);
            var result = schedules.Select(s => MapToDto(s)).ToList();
            return Response<List<ScheduleDto>>.Success(result, 200);
        }

        public async Task<Response<ScheduleDto>> CreateScheduleAsync(ScheduleCreateDto dto)
        {
            var conflicts = await CheckConflictsAsync(dto);
            if (conflicts.Data != null && conflicts.Data.Any())
                return Response<ScheduleDto>.Fail("Çakışma tespit edildi: " + conflicts.Data.First().Message, 400);

            var section = await _unitOfWork.CourseSections.GetByIdAsync(dto.SectionId);
            if (section == null)
                return Response<ScheduleDto>.Fail("Ders bölümü bulunamadı", 404);

            var classroom = await _unitOfWork.Classrooms.GetByIdAsync(dto.ClassroomId);
            if (classroom == null)
                return Response<ScheduleDto>.Fail("Sınıf bulunamadı", 404);

            var schedule = new Schedule
            {
                SectionId = dto.SectionId,
                ClassroomId = dto.ClassroomId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Schedules.AddAsync(schedule);
            await _unitOfWork.CommitAsync();

            var result = await _unitOfWork.Schedules.GetByIdWithDetailsAsync(schedule.Id);
            return Response<ScheduleDto>.Success(MapToDto(result!), 201);
        }

        public async Task<Response<ScheduleDto>> UpdateScheduleAsync(int id, ScheduleUpdateDto dto)
        {
            var schedule = await _unitOfWork.Schedules.GetByIdAsync(id);
            if (schedule == null)
                return Response<ScheduleDto>.Fail("Program bulunamadı", 404);

            var checkDto = new ScheduleCreateDto
            {
                SectionId = dto.SectionId ?? schedule.SectionId,
                ClassroomId = dto.ClassroomId ?? schedule.ClassroomId,
                DayOfWeek = dto.DayOfWeek ?? schedule.DayOfWeek,
                StartTime = dto.StartTime ?? schedule.StartTime,
                EndTime = dto.EndTime ?? schedule.EndTime
            };

            var conflicts = await CheckConflictsAsync(checkDto, id);
            if (conflicts.Data != null && conflicts.Data.Any())
                return Response<ScheduleDto>.Fail("Çakışma tespit edildi: " + conflicts.Data.First().Message, 400);

            if (dto.SectionId.HasValue) schedule.SectionId = dto.SectionId.Value;
            if (dto.ClassroomId.HasValue) schedule.ClassroomId = dto.ClassroomId.Value;
            if (dto.DayOfWeek.HasValue) schedule.DayOfWeek = dto.DayOfWeek.Value;
            if (dto.StartTime.HasValue) schedule.StartTime = dto.StartTime.Value;
            if (dto.EndTime.HasValue) schedule.EndTime = dto.EndTime.Value;

            schedule.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.Schedules.Update(schedule);
            await _unitOfWork.CommitAsync();

            var result = await _unitOfWork.Schedules.GetByIdWithDetailsAsync(id);
            return Response<ScheduleDto>.Success(MapToDto(result!), 200);
        }

        public async Task<Response<NoDataDto>> DeleteScheduleAsync(int id)
        {
            var schedule = await _unitOfWork.Schedules.GetByIdAsync(id);
            if (schedule == null)
                return Response<NoDataDto>.Fail("Program bulunamadı", 404);

            schedule.IsActive = false;
            schedule.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.Schedules.Update(schedule);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<List<ScheduleConflictDto>>> CheckConflictsAsync(ScheduleCreateDto dto, int? excludeId = null)
        {
            var conflicts = new List<ScheduleConflictDto>();

            var classroomConflict = await _unitOfWork.Schedules.GetConflictingScheduleAsync(
                dto.ClassroomId, dto.DayOfWeek, dto.StartTime, dto.EndTime, excludeId);

            if (classroomConflict != null)
            {
                conflicts.Add(new ScheduleConflictDto
                {
                    ConflictType = "Sınıf",
                    ConflictingScheduleId = classroomConflict.Id,
                    ConflictingCourse = $"{classroomConflict.Section?.Course?.Code} - {classroomConflict.Section?.Course?.Name}",
                    Message = $"Bu sınıf {dto.DayOfWeek} günü {dto.StartTime:hh\\:mm}-{dto.EndTime:hh\\:mm} saatlerinde başka bir ders için kullanılıyor."
                });
            }

            var sectionConflict = await _unitOfWork.Schedules.GetSectionConflictAsync(
                dto.SectionId, dto.DayOfWeek, dto.StartTime, dto.EndTime, excludeId);

            if (sectionConflict != null)
            {
                conflicts.Add(new ScheduleConflictDto
                {
                    ConflictType = "Ders Bölümü",
                    ConflictingScheduleId = sectionConflict.Id,
                    Message = $"Bu ders bölümü zaten {dto.DayOfWeek} günü {dto.StartTime:hh\\:mm}-{dto.EndTime:hh\\:mm} saatlerinde programlı."
                });
            }

            return Response<List<ScheduleConflictDto>>.Success(conflicts, 200);
        }

        private static ScheduleDto MapToDto(Schedule s)
        {
            return new ScheduleDto
            {
                Id = s.Id,
                SectionId = s.SectionId,
                CourseCode = s.Section?.Course?.Code ?? "",
                CourseName = s.Section?.Course?.Name ?? "",
                SectionNumber = s.Section?.SectionNumber ?? "",
                ClassroomId = s.ClassroomId,
                ClassroomInfo = s.Classroom != null ? $"{s.Classroom.Building} - {s.Classroom.RoomNumber}" : "",
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                InstructorName = s.Section?.Instructor?.User?.UserName
            };
        }
    }
}
