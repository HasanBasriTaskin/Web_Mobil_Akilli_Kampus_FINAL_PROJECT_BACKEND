using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Tools;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/attendance/excuse-requests")]
    [ApiController]
    [Authorize]
    public class ExcuseRequestsController : ControllerBase
    {
        private readonly IExcuseRequestService _excuseRequestService;
        private readonly UserClaimsHelper _userClaimsHelper;

        public ExcuseRequestsController(IExcuseRequestService excuseRequestService, UserClaimsHelper userClaimsHelper)
        {
            _excuseRequestService = excuseRequestService;
            _userClaimsHelper = userClaimsHelper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateExcuseRequest([FromBody] ExcuseRequestCreateDto createDto)
        {
            var studentId = await _userClaimsHelper.GetStudentIdAsync();
            if (!studentId.HasValue)
                return Unauthorized("Student not found or user is not a student");

            var result = await _excuseRequestService.CreateExcuseRequestAsync(studentId.Value, createDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> GetExcuseRequests()
        {
            var instructorId = _userClaimsHelper.GetUserId();
            var result = await _excuseRequestService.GetExcuseRequestsAsync(instructorId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> ApproveExcuseRequest(int id, [FromBody] ExcuseRequestReviewDto reviewDto)
        {
            var instructorId = _userClaimsHelper.GetUserId();
            if (string.IsNullOrEmpty(instructorId))
                return Unauthorized("Instructor not found");

            var result = await _excuseRequestService.ApproveExcuseRequestAsync(id, instructorId, reviewDto.Notes);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> RejectExcuseRequest(int id, [FromBody] ExcuseRequestReviewDto reviewDto)
        {
            var instructorId = _userClaimsHelper.GetUserId();
            if (string.IsNullOrEmpty(instructorId))
                return Unauthorized("Instructor not found");

            var result = await _excuseRequestService.RejectExcuseRequestAsync(id, instructorId, reviewDto.Notes);
            return StatusCode(result.StatusCode, result);
        }
    }
}

