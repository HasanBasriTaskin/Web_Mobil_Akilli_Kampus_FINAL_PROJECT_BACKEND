using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class ScheduleManager : IScheduleService
    {
        private readonly CampusContext _context;

        public ScheduleManager(CampusContext context)
        {
            _context = context;
        }

        public async Task<Response<List<ScheduleDto>>> GetSchedulesBySectionAsync(int sectionId)
        {
            var schedules = await _context.Schedules
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Include(s => s.Classroom)
                .Where(s => s.SectionId == sectionId && s.IsActive)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            var result = schedules.Select(s => MapToDto(s)).ToList();
            return Response<List<ScheduleDto>>.Success(result, 200);
        }

        public async Task<Response<List<WeeklyScheduleDto>>> GetWeeklyScheduleAsync(int sectionId)
        {
            var section = await _context.CourseSections
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null)
                return Response<List<WeeklyScheduleDto>>.Fail("Ders bölümü bulunamadı", 404);

            var schedules = await _context.Schedules
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Include(s => s.Classroom)
                .Where(s => s.SectionId == sectionId && s.IsActive)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            var result = schedules
                .GroupBy(s => s.DayOfWeek)
                .Select(g => new WeeklyScheduleDto
                {
                    Day = g.Key,
                    Schedules = g.Select(s => MapToDto(s)).ToList()
                })
                .ToList();

            return Response<List<WeeklyScheduleDto>>.Success(result, 200);
        }

        public async Task<Response<List<ScheduleDto>>> GetSchedulesByClassroomAsync(int classroomId, DayOfWeek? dayOfWeek = null)
        {
            var query = _context.Schedules
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Include(s => s.Classroom)
                .Where(s => s.ClassroomId == classroomId && s.IsActive);

            if (dayOfWeek.HasValue)
                query = query.Where(s => s.DayOfWeek == dayOfWeek.Value);

            var schedules = await query
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            var result = schedules.Select(s => MapToDto(s)).ToList();
            return Response<List<ScheduleDto>>.Success(result, 200);
        }

        public async Task<Response<List<ScheduleDto>>> GetSchedulesByInstructorAsync(int facultyId, DayOfWeek? dayOfWeek = null)
        {
            var query = _context.Schedules
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Instructor)
                .Include(s => s.Classroom)
                .Where(s => s.Section.InstructorId == facultyId && s.IsActive);

            if (dayOfWeek.HasValue)
                query = query.Where(s => s.DayOfWeek == dayOfWeek.Value);

            var schedules = await query
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            var result = schedules.Select(s => MapToDto(s)).ToList();
            return Response<List<ScheduleDto>>.Success(result, 200);
        }

        public async Task<Response<ScheduleDto>> CreateScheduleAsync(ScheduleCreateDto dto)
        {
            // Çakışma kontrolü
            var conflicts = await CheckConflictsAsync(dto);
            if (conflicts.Data != null && conflicts.Data.Any())
                return Response<ScheduleDto>.Fail("Çakışma tespit edildi: " + conflicts.Data.First().Message, 400);

            // Section kontrolü
            var section = await _context.CourseSections.FindAsync(dto.SectionId);
            if (section == null)
                return Response<ScheduleDto>.Fail("Ders bölümü bulunamadı", 404);

            // Classroom kontrolü
            var classroom = await _context.Classrooms.FindAsync(dto.ClassroomId);
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

            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            var result = await _context.Schedules
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Include(s => s.Classroom)
                .FirstAsync(s => s.Id == schedule.Id);

            return Response<ScheduleDto>.Success(MapToDto(result), 201);
        }

        public async Task<Response<ScheduleDto>> UpdateScheduleAsync(int id, ScheduleUpdateDto dto)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
                return Response<ScheduleDto>.Fail("Program bulunamadı", 404);

            // Geçici DTO ile çakışma kontrolü
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
            await _context.SaveChangesAsync();

            var result = await _context.Schedules
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Include(s => s.Classroom)
                .FirstAsync(s => s.Id == id);

            return Response<ScheduleDto>.Success(MapToDto(result), 200);
        }

        public async Task<Response<NoDataDto>> DeleteScheduleAsync(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
                return Response<NoDataDto>.Fail("Program bulunamadı", 404);

            schedule.IsActive = false;
            schedule.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<List<ScheduleConflictDto>>> CheckConflictsAsync(ScheduleCreateDto dto, int? excludeId = null)
        {
            var conflicts = new List<ScheduleConflictDto>();

            // Sınıf çakışması kontrolü
            var classroomConflict = await _context.Schedules
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Where(s => s.ClassroomId == dto.ClassroomId &&
                           s.DayOfWeek == dto.DayOfWeek &&
                           s.IsActive &&
                           (excludeId == null || s.Id != excludeId))
                .Where(s => (s.StartTime < dto.EndTime && s.EndTime > dto.StartTime))
                .FirstOrDefaultAsync();

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

            // Aynı section için çakışma kontrolü
            var sectionConflict = await _context.Schedules
                .Where(s => s.SectionId == dto.SectionId &&
                           s.DayOfWeek == dto.DayOfWeek &&
                           s.IsActive &&
                           (excludeId == null || s.Id != excludeId))
                .Where(s => (s.StartTime < dto.EndTime && s.EndTime > dto.StartTime))
                .FirstOrDefaultAsync();

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
