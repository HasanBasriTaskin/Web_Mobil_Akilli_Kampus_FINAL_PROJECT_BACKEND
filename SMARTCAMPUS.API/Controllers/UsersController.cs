using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.User;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize] // Require Auth for all user operations. 
    // Ideally [Authorize(Roles = "Admin")] for most, but let's stick to basic Auth for now or allow user to see their own?
    // Let's assume Admin only for List/Delete.
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var result = await _userService.GetUserByIdAsync(currentUserId);
            if (!result.IsSuccessful) return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UserUpdateDto userUpdateDto)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
             if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var result = await _userService.UpdateUserAsync(currentUserId, userUpdateDto);
            if (!result.IsSuccessful) return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        [HttpPost("me/profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var result = await _userService.UploadProfilePictureAsync(currentUserId, file);
            if (!result.IsSuccessful) return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")] // Only Admin can list users
        public async Task<IActionResult> GetUsers([FromQuery] UserQueryParameters queryParams)
        {
            var result = await _userService.GetUsersAsync(queryParams);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Allow if Admin OR accessing own data
            if (!isAdmin && currentUserId != id)
            {
                return StatusCode(403, "Access Denied: You can only view your own profile.");
            }

            var result = await _userService.GetUserByIdAsync(id);
            if (!result.IsSuccessful) return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDto userUpdateDto)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Allow if Admin OR accessing own data
            if (!isAdmin && currentUserId != id)
            {
                return StatusCode(403, "Access Denied: You can only update your own profile.");
            }

            var result = await _userService.UpdateUserAsync(id, userUpdateDto);
            if (!result.IsSuccessful) return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result.IsSuccessful) return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        [HttpPost("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRoles(string id, [FromBody] List<string> roles)
        {
            var result = await _userService.AssignRolesAsync(id, roles);
            if (!result.IsSuccessful) return StatusCode(result.StatusCode, result);
            return Ok(result);
        }
    }
}
