using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Cafeteria;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class CafeteriasController : ControllerBase
    {
        private readonly ICafeteriaService _cafeteriaService;

        public CafeteriasController(ICafeteriaService cafeteriaService)
        {
            _cafeteriaService = cafeteriaService;
        }

        /// <summary>
        /// Tüm yemekhane listesini getirir
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            // Sadece admin'ler inactive'leri görebilir
            var isAdmin = User.IsInRole("Admin");
            var result = await _cafeteriaService.GetAllAsync(isAdmin && includeInactive);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// ID'ye göre yemekhane detayını getirir
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _cafeteriaService.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Yeni yemekhane oluşturur
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CafeteriaCreateDto dto)
        {
            var result = await _cafeteriaService.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Yemekhane bilgilerini günceller
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CafeteriaUpdateDto dto)
        {
            var result = await _cafeteriaService.UpdateAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Yemekhaneyi siler (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _cafeteriaService.DeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
