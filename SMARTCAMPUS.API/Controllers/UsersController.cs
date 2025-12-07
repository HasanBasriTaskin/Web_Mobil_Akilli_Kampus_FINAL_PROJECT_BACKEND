using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.User;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/[controller]")]
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

        [HttpGet]
        [Authorize(Roles = "Admin")] // Only Admin can list users
        public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetUsersAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            // TODO: Check if current user is Admin OR accessing their own data
            var result = await _userService.GetUserByIdAsync(id);
            if (!result.IsSuccessful) return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDto userUpdateDto)
        {
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
