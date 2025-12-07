using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            if (!result.IsSuccessful)
            {
                // We could use a base controller method to handle Response<T> mapping
                // For now, manual mapping based on StatusCode
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpPost("register-student")]
        public async Task<IActionResult> RegisterStudent(RegisterStudentDto registerDto)
        {
            var result = await _authService.RegisterStudentAsync(registerDto);
            if (!result.IsSuccessful)
            {
                return StatusCode(result.StatusCode, result);
            }
            return StatusCode(201, result);
        }
    }
}
