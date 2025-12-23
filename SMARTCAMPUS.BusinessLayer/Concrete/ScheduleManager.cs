using System.Diagnostics;
using System.Text;
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

        #region Automatic Schedule Generation (CSP/Backtracking)

        /// <summary>
        /// CSP (Constraint Satisfaction Problem) yaklaşımı ile otomatik ders programı oluşturur.
        /// Backtracking algoritması ile MRV ve LCV heuristikleri kullanır.
        /// </summary>
        public async Task<Response<AutoScheduleResultDto>> GenerateAutomaticScheduleAsync(AutoScheduleRequestDto dto)
        {
            var stopwatch = Stopwatch.StartNew();
            var statistics = new AlgorithmStatisticsDto();

            // 1. Verileri hazırla
            var sections = dto.SectionIds != null && dto.SectionIds.Any()
                ? (await _unitOfWork.CourseSections.GetSectionsBySemesterAsync(dto.Semester, dto.Year))
                    .Where(s => dto.SectionIds.Contains(s.Id)).ToList()
                : await _unitOfWork.CourseSections.GetSectionsBySemesterAsync(dto.Semester, dto.Year);

            if (!sections.Any())
            {
                return Response<AutoScheduleResultDto>.Success(new AutoScheduleResultDto
                {
                    IsSuccess = false,
                    Message = "Belirtilen dönem için ders bölümü bulunamadı",
                    TotalSections = 0
                }, 200);
            }

            // Zaten programlanmış bölümleri filtrele
            var unscheduledSections = sections.Where(s => !s.Schedules.Any(sch => sch.IsActive)).ToList();

            var classrooms = await _unitOfWork.Classrooms.GetAllActiveAsync();
            if (!classrooms.Any())
            {
                return Response<AutoScheduleResultDto>.Fail("Aktif sınıf bulunamadı", 400);
            }

            // Varsayılan zaman slotları
            var timeSlots = dto.AllowedTimeSlots ?? GetDefaultTimeSlots();

            // 2. Domain oluştur (her bölüm için olası atamalar)
            var domain = new Dictionary<int, List<(int ClassroomId, DayOfWeek Day, TimeSpan Start, TimeSpan End)>>();
            foreach (var section in unscheduledSections)
            {
                var possibleAssignments = new List<(int, DayOfWeek, TimeSpan, TimeSpan)>();
                foreach (var classroom in classrooms.Where(c => c.Capacity >= section.Capacity))
                {
                    foreach (var slot in timeSlots)
                    {
                        possibleAssignments.Add((classroom.Id, slot.Day, slot.StartTime, slot.EndTime));
                    }
                }
                domain[section.Id] = possibleAssignments;
            }

            // 3. Backtracking ile çöz
            var assignments = new Dictionary<int, (int ClassroomId, DayOfWeek Day, TimeSpan Start, TimeSpan End)>();
            var failedSections = new List<UnscheduledSectionDto>();
            var createdSchedules = new List<Schedule>();

            var success = await BacktrackingSchedule(
                unscheduledSections, 
                domain, 
                assignments, 
                0, 
                dto.MaxIterations, 
                statistics);

            stopwatch.Stop();
            statistics.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            // 4. Başarılı atamaları kaydet
            foreach (var assignment in assignments)
            {
                var section = unscheduledSections.First(s => s.Id == assignment.Key);
                var schedule = new Schedule
                {
                    SectionId = assignment.Key,
                    ClassroomId = assignment.Value.ClassroomId,
                    DayOfWeek = assignment.Value.Day,
                    StartTime = assignment.Value.Start,
                    EndTime = assignment.Value.End,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Schedules.AddAsync(schedule);
                createdSchedules.Add(schedule);
            }

            await _unitOfWork.CommitAsync();

            // 5. Programlanamayan bölümleri belirle
            foreach (var section in unscheduledSections.Where(s => !assignments.ContainsKey(s.Id)))
            {
                failedSections.Add(new UnscheduledSectionDto
                {
                    SectionId = section.Id,
                    CourseCode = section.Course?.Code ?? "",
                    CourseName = section.Course?.Name ?? "",
                    SectionNumber = section.SectionNumber,
                    Reason = "Uygun zaman slotu veya sınıf bulunamadı"
                });
            }

            // 6. Kayıtlı programları detaylarıyla getir
            var generatedSchedules = new List<ScheduleDto>();
            foreach (var schedule in createdSchedules)
            {
                var fullSchedule = await _unitOfWork.Schedules.GetByIdWithDetailsAsync(schedule.Id);
                if (fullSchedule != null)
                    generatedSchedules.Add(MapToDto(fullSchedule));
            }

            var result = new AutoScheduleResultDto
            {
                IsSuccess = failedSections.Count == 0,
                Message = failedSections.Count == 0 
                    ? "Tüm dersler başarıyla programlandı" 
                    : $"{assignments.Count} ders programlandı, {failedSections.Count} ders programlanamadı",
                TotalSections = unscheduledSections.Count,
                ScheduledSections = assignments.Count,
                UnscheduledSections = failedSections.Count,
                GeneratedSchedules = generatedSchedules,
                FailedSections = failedSections,
                Statistics = statistics
            };

            return Response<AutoScheduleResultDto>.Success(result, 200);
        }

        /// <summary>
        /// Backtracking algoritması - recursive
        /// </summary>
        private async Task<bool> BacktrackingSchedule(
            List<CourseSection> sections,
            Dictionary<int, List<(int ClassroomId, DayOfWeek Day, TimeSpan Start, TimeSpan End)>> domain,
            Dictionary<int, (int ClassroomId, DayOfWeek Day, TimeSpan Start, TimeSpan End)> assignments,
            int iterationCount,
            int maxIterations,
            AlgorithmStatisticsDto statistics)
        {
            statistics.TotalIterations = iterationCount;

            if (iterationCount >= maxIterations)
                return false;

            // Tüm bölümler atandı mı?
            if (assignments.Count == sections.Count)
                return true;

            // MRV (Minimum Remaining Values) Heuristic: En az seçeneği olan bölümü seç
            var unassigned = sections.Where(s => !assignments.ContainsKey(s.Id)).ToList();
            var selectedSection = SelectUnassignedVariable(unassigned, domain, assignments);

            if (selectedSection == null || !domain.ContainsKey(selectedSection.Id))
                return false;

            // LCV (Least Constraining Value) Heuristic: En az kısıtlayan değeri seç
            var orderedDomain = OrderDomainValues(selectedSection.Id, domain, assignments, sections);

            foreach (var value in orderedDomain)
            {
                // Kısıt kontrolü
                if (await IsConsistentAssignment(selectedSection, value, assignments, sections))
                {
                    assignments[selectedSection.Id] = value;

                    var result = await BacktrackingSchedule(
                        sections, domain, assignments, iterationCount + 1, maxIterations, statistics);

                    if (result)
                        return true;

                    // Backtrack
                    assignments.Remove(selectedSection.Id);
                    statistics.BacktrackCount++;
                }
            }

            return false;
        }

        /// <summary>
        /// MRV Heuristic: En az seçeneği olan bölümü seç
        /// </summary>
        private CourseSection? SelectUnassignedVariable(
            List<CourseSection> unassigned,
            Dictionary<int, List<(int, DayOfWeek, TimeSpan, TimeSpan)>> domain,
            Dictionary<int, (int, DayOfWeek, TimeSpan, TimeSpan)> assignments)
        {
            return unassigned
                .Where(s => domain.ContainsKey(s.Id))
                .OrderBy(s => domain[s.Id].Count)
                .ThenByDescending(s => s.Capacity) // Önce büyük kapasiteli dersleri yerleştir
                .FirstOrDefault();
        }

        /// <summary>
        /// LCV Heuristic: En az kısıtlayan değeri önce dene
        /// </summary>
        private List<(int ClassroomId, DayOfWeek Day, TimeSpan Start, TimeSpan End)> OrderDomainValues(
            int sectionId,
            Dictionary<int, List<(int, DayOfWeek, TimeSpan, TimeSpan)>> domain,
            Dictionary<int, (int, DayOfWeek, TimeSpan, TimeSpan)> assignments,
            List<CourseSection> allSections)
        {
            if (!domain.ContainsKey(sectionId))
                return new List<(int, DayOfWeek, TimeSpan, TimeSpan)>();

            // Sabah saatlerini tercih et (esnek kısıt)
            return domain[sectionId]
                .OrderBy(v => v.Item3) // Erken saatleri tercih et (Start = Item3)
                .ThenBy(v => (int)v.Item2) // Pazartesi'den başla (Day = Item2)
                .ToList();
        }

        /// <summary>
        /// Kısıt kontrolü: Bu atama tutarlı mı?
        /// </summary>
        private async Task<bool> IsConsistentAssignment(
            CourseSection section,
            (int ClassroomId, DayOfWeek Day, TimeSpan Start, TimeSpan End) value,
            Dictionary<int, (int ClassroomId, DayOfWeek Day, TimeSpan Start, TimeSpan End)> assignments,
            List<CourseSection> allSections)
        {
            // Hard Constraint 1: Sınıf çakışması kontrolü
            foreach (var assignment in assignments)
            {
                if (assignment.Value.ClassroomId == value.ClassroomId &&
                    assignment.Value.Day == value.Day &&
                    TimesOverlap(assignment.Value.Start, assignment.Value.End, value.Start, value.End))
                {
                    return false;
                }
            }

            // Hard Constraint 2: Eğitmen çakışması kontrolü
            var instructorId = section.InstructorId;
            foreach (var assignment in assignments)
            {
                var assignedSection = allSections.First(s => s.Id == assignment.Key);
                if (assignedSection.InstructorId == instructorId &&
                    assignment.Value.Day == value.Day &&
                    TimesOverlap(assignment.Value.Start, assignment.Value.End, value.Start, value.End))
                {
                    return false;
                }
            }

            // Veritabanında mevcut çakışma kontrolü
            var dbConflict = await _unitOfWork.Schedules.HasConflictAsync(
                value.ClassroomId, value.Day, value.Start, value.End, null);

            return !dbConflict;
        }

        /// <summary>
        /// İki zaman aralığının çakışıp çakışmadığını kontrol eder
        /// </summary>
        private bool TimesOverlap(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
        {
            return start1 < end2 && start2 < end1;
        }

        /// <summary>
        /// Varsayılan zaman slotları
        /// </summary>
        private List<TimeSlotDefinitionDto> GetDefaultTimeSlots()
        {
            var slots = new List<TimeSlotDefinitionDto>();
            var days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
            var times = new[]
            {
                (new TimeSpan(8, 30, 0), new TimeSpan(9, 20, 0)),
                (new TimeSpan(9, 30, 0), new TimeSpan(10, 20, 0)),
                (new TimeSpan(10, 30, 0), new TimeSpan(11, 20, 0)),
                (new TimeSpan(11, 30, 0), new TimeSpan(12, 20, 0)),
                (new TimeSpan(13, 30, 0), new TimeSpan(14, 20, 0)),
                (new TimeSpan(14, 30, 0), new TimeSpan(15, 20, 0)),
                (new TimeSpan(15, 30, 0), new TimeSpan(16, 20, 0)),
                (new TimeSpan(16, 30, 0), new TimeSpan(17, 20, 0))
            };

            foreach (var day in days)
            {
                foreach (var (start, end) in times)
                {
                    slots.Add(new TimeSlotDefinitionDto { Day = day, StartTime = start, EndTime = end });
                }
            }

            return slots;
        }

        #endregion

        #region iCal Export

        /// <summary>
        /// Bir ders bölümünün programını iCal formatında dışa aktarır
        /// </summary>
        public async Task<Response<string>> ExportSectionToICalAsync(int sectionId)
        {
            var section = await _unitOfWork.CourseSections.GetSectionWithDetailsAsync(sectionId);
            if (section == null)
                return Response<string>.Fail("Ders bölümü bulunamadı", 404);

            var schedules = await _unitOfWork.Schedules.GetBySectionIdAsync(sectionId);
            if (!schedules.Any())
                return Response<string>.Fail("Bu bölüm için program bulunamadı", 404);

            var ical = GenerateICalContent(schedules, section.Course?.Name ?? "Ders", section.Semester, section.Year);
            return Response<string>.Success(ical, 200);
        }

        /// <summary>
        /// Bir öğrencinin tüm ders programını iCal formatında dışa aktarır
        /// </summary>
        public async Task<Response<string>> ExportStudentScheduleToICalAsync(string userId)
        {
            var student = await _unitOfWork.Students.GetByUserIdAsync(userId);
            if (student == null)
                return Response<string>.Fail("Öğrenci bulunamadı", 404);

            // Öğrencinin kayıtlı olduğu bölümleri bul
            var enrollments = (await _unitOfWork.Enrollments.GetEnrollmentsByStudentAsync(student.Id)).ToList();
            if (!enrollments.Any())
                return Response<string>.Fail("Kayıtlı ders bulunamadı", 404);

            var sectionIds = enrollments.Select(e => e.SectionId).ToList();
            var allSchedules = await _unitOfWork.Schedules.GetSchedulesBySectionIdsAsync(sectionIds);

            if (!allSchedules.Any())
                return Response<string>.Fail("Program bulunamadı", 404);

            var ical = GenerateICalContent(allSchedules, "Ders Programı", "", 0);
            return Response<string>.Success(ical, 200);
        }

        /// <summary>
        /// iCal içeriği oluşturur
        /// </summary>
        private string GenerateICalContent(List<Schedule> schedules, string calendarName, string semester, int year)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//Smart Campus//Schedule//TR");
            sb.AppendLine($"X-WR-CALNAME:{calendarName}");
            sb.AppendLine("CALSCALE:GREGORIAN");
            sb.AppendLine("METHOD:PUBLISH");

            // Dönemin başlangıç tarihini belirle
            var semesterStart = GetSemesterStartDate(semester, year);

            foreach (var schedule in schedules.Where(s => s.IsActive))
            {
                // İlk ders gününü bul
                var firstOccurrence = GetFirstOccurrence(semesterStart, schedule.DayOfWeek);

                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine($"UID:{Guid.NewGuid()}@smartcampus.edu");
                sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");

                var startDateTime = firstOccurrence.Add(schedule.StartTime);
                var endDateTime = firstOccurrence.Add(schedule.EndTime);

                sb.AppendLine($"DTSTART:{startDateTime:yyyyMMddTHHmmss}");
                sb.AppendLine($"DTEND:{endDateTime:yyyyMMddTHHmmss}");

                // Her hafta tekrarla (16 hafta)
                sb.AppendLine("RRULE:FREQ=WEEKLY;COUNT=16");

                var summary = schedule.Section?.Course != null
                    ? $"{schedule.Section.Course.Code} - {schedule.Section.Course.Name}"
                    : "Ders";

                var location = schedule.Classroom != null
                    ? $"{schedule.Classroom.Building} - {schedule.Classroom.RoomNumber}"
                    : "";

                sb.AppendLine($"SUMMARY:{EscapeICalText(summary)}");
                sb.AppendLine($"LOCATION:{EscapeICalText(location)}");

                if (schedule.Section?.Instructor?.User != null)
                {
                    sb.AppendLine($"DESCRIPTION:Öğretim Üyesi: {EscapeICalText(schedule.Section.Instructor.User.FullName)}");
                }

                sb.AppendLine("END:VEVENT");
            }

            sb.AppendLine("END:VCALENDAR");
            return sb.ToString();
        }

        private DateTime GetSemesterStartDate(string semester, int year)
        {
            if (year == 0) year = DateTime.Now.Year;

            return semester?.ToLower() switch
            {
                "fall" => new DateTime(year, 9, 1),
                "spring" => new DateTime(year, 2, 1),
                "summer" => new DateTime(year, 6, 15),
                _ => DateTime.Now
            };
        }

        private DateTime GetFirstOccurrence(DateTime start, DayOfWeek targetDay)
        {
            var daysUntilTarget = ((int)targetDay - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysUntilTarget);
        }

        private string EscapeICalText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("\\", "\\\\").Replace(",", "\\,").Replace(";", "\\;").Replace("\n", "\\n");
        }

        #endregion

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
