using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Tools;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;
        private readonly UserClaimsHelper _userClaimsHelper;

        public AnnouncementsController(IAnnouncementService announcementService, UserClaimsHelper userClaimsHelper)
        {
            _announcementService = announcementService;
            _userClaimsHelper = userClaimsHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnnouncements([FromQuery] string? targetAudience, [FromQuery] int? departmentId)
        {
            // If student, get their department
            var studentId = await _userClaimsHelper.GetStudentIdAsync();
            int? studentDepartmentId = null;
            
            if (studentId.HasValue)
            {
                var student = await _userClaimsHelper.GetStudentWithDetailsAsync(studentId.Value);
                studentDepartmentId = student?.DepartmentId;
            }

            var result = await _announcementService.GetAnnouncementsAsync(targetAudience, departmentId ?? studentDepartmentId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("important")]
        public async Task<IActionResult> GetImportantAnnouncements()
        {
            var result = await _announcementService.GetImportantAnnouncementsAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnnouncementById(int id)
        {
            var result = await _announcementService.GetAnnouncementByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/view")]
        public async Task<IActionResult> IncrementViewCount(int id)
        {
            var result = await _announcementService.IncrementViewCountAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
