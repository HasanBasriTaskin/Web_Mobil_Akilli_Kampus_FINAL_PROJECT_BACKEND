using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public SchedulesController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        /// <summary>
        /// Section'a göre ders programını getirir
        /// </summary>
        [HttpGet("section/{sectionId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySection(int sectionId)
        {
            var result = await _scheduleService.GetSchedulesBySectionAsync(sectionId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Haftalık ders programını getirir
        /// </summary>
        [HttpGet("section/{sectionId}/weekly")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWeeklySchedule(int sectionId)
        {
            var result = await _scheduleService.GetWeeklyScheduleAsync(sectionId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Sınıfa göre ders programını getirir
        /// </summary>
        [HttpGet("classroom/{classroomId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByClassroom(int classroomId, [FromQuery] DayOfWeek? dayOfWeek)
        {
            var result = await _scheduleService.GetSchedulesByClassroomAsync(classroomId, dayOfWeek);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Öğretim üyesine göre ders programını getirir
        /// </summary>
        [HttpGet("instructor/{facultyId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByInstructor(int facultyId, [FromQuery] DayOfWeek? dayOfWeek)
        {
            var result = await _scheduleService.GetSchedulesByInstructorAsync(facultyId, dayOfWeek);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Yeni ders programı oluşturur
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] ScheduleCreateDto dto)
        {
            var result = await _scheduleService.CreateScheduleAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Ders programını günceller
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ScheduleUpdateDto dto)
        {
            var result = await _scheduleService.UpdateScheduleAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Ders programını siler
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _scheduleService.DeleteScheduleAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Çakışma kontrolü yapar
        /// </summary>
        [HttpPost("check-conflicts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckConflicts([FromBody] ScheduleCreateDto dto, [FromQuery] int? excludeId)
        {
            var result = await _scheduleService.CheckConflictsAsync(dto, excludeId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Otomatik ders programı oluşturur (CSP/Backtracking algoritması)
        /// </summary>
        /// <remarks>
        /// Bu endpoint, belirtilen dönem için tüm ders bölümlerini otomatik olarak programlar.
        /// Backtracking algoritması ile MRV ve LCV heuristikleri kullanır.
        /// 
        /// Hard Constraints:
        /// - Sınıf çakışması olmamalı
        /// - Eğitmen çakışması olmamalı
        /// 
        /// Soft Constraints:
        /// - Sabah saatleri tercih edilir
        /// - Büyük kapasiteli dersler öncelikli yerleştirilir
        /// </remarks>
        [HttpPost("generate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateAutomaticSchedule([FromBody] AutoScheduleRequestDto dto)
        {
            var result = await _scheduleService.GenerateAutomaticScheduleAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Bir ders bölümünün programını iCal formatında dışa aktarır
        /// </summary>
        [HttpGet("section/{sectionId}/ical")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportSectionToICal(int sectionId)
        {
            var result = await _scheduleService.ExportSectionToICalAsync(sectionId);
            if (!result.IsSuccessful)
                return StatusCode(result.StatusCode, result);

            return File(
                System.Text.Encoding.UTF8.GetBytes(result.Data!), 
                "text/calendar", 
                $"section_{sectionId}_schedule.ics");
        }

        /// <summary>
        /// Giriş yapan öğrencinin ders programını iCal formatında dışa aktarır
        /// </summary>
        [HttpGet("my-schedule/ical")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ExportMyScheduleToICal()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _scheduleService.ExportStudentScheduleToICalAsync(userId);
            if (!result.IsSuccessful)
                return StatusCode(result.StatusCode, result);

            return File(
                System.Text.Encoding.UTF8.GetBytes(result.Data!), 
                "text/calendar", 
                "my_schedule.ics");
        }
    }
}

