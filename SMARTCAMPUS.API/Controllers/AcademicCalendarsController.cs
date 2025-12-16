using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class AcademicCalendarsController : ControllerBase
    {
        private readonly IAcademicCalendarService _academicCalendarService;

        public AcademicCalendarsController(IAcademicCalendarService academicCalendarService)
        {
            _academicCalendarService = academicCalendarService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCalendars([FromQuery] int? year, [FromQuery] string? semester)
        {
            var result = await _academicCalendarService.GetCalendarsAsync(year, semester);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("date-range")]
        public async Task<IActionResult> GetCalendarsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _academicCalendarService.GetCalendarsByDateRangeAsync(startDate, endDate);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingEvents([FromQuery] int days = 30)
        {
            var result = await _academicCalendarService.GetUpcomingEventsAsync(days);
            return StatusCode(result.StatusCode, result);
        }
    }
}
