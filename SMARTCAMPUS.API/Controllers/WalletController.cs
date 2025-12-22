using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Wallet;
using System.Security.Claims;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        /// <summary>
        /// Kullanıcının cüzdan bilgilerini getirir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWallet()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _walletService.GetWalletAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// İşlem geçmişini getirir (sayfalı)
        /// </summary>
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _walletService.GetTransactionsAsync(userId, page, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Bakiye yükler (kredi kartı ile)
        /// </summary>
        [HttpPost("topup")]
        public async Task<IActionResult> TopUp([FromBody] WalletTopUpDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _walletService.TopUpAsync(userId, dto);
            return StatusCode(result.StatusCode, result);
        }

        #region Admin Operations

        /// <summary>
        /// Kullanıcının cüzdanını ID ile getirir (Admin)
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserWallet(string userId)
        {
            var result = await _walletService.GetWalletByUserIdAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Cüzdan durumunu değiştirir (Admin)
        /// </summary>
        [HttpPut("user/{userId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetStatus(string userId, [FromQuery] bool isActive)
        {
            var result = await _walletService.SetWalletStatusAsync(userId, isActive);
            return StatusCode(result.StatusCode, result);
        }

        #endregion

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
