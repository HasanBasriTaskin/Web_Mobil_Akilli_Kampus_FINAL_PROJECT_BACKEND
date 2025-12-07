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

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterStudentDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);
            if (!result.IsSuccessful)
            {
                return StatusCode(result.StatusCode, result);
            }
            return StatusCode(201, result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var result = await _authService.ResetPasswordAsync(resetPasswordDto);
            if (!result.IsSuccessful)
            {
                 return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> CreateTokenByRefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var result = await _authService.CreateTokenByRefreshTokenAsync(refreshTokenDto.Token);
            if (!result.IsSuccessful)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var result = await _authService.RevokeRefreshTokenAsync(refreshTokenDto.Token);
            if (!result.IsSuccessful)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await _authService.VerifyEmailAsync(userId, token);
            if (!result.IsSuccessful)
            {
                 return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }
    }
}
