using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Event;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class EventCategoriesController : ControllerBase
    {
        private readonly IEventCategoryService _categoryService;

        public EventCategoriesController(IEventCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Tüm etkinlik kategorilerini getirir
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            var isAdmin = User.IsInRole("Admin");
            var result = await _categoryService.GetAllAsync(isAdmin && includeInactive);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// ID'ye göre kategori detayını getirir
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Yeni kategori oluşturur
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EventCategoryCreateDto dto)
        {
            var result = await _categoryService.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Kategoriyi günceller
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EventCategoryUpdateDto dto)
        {
            var result = await _categoryService.UpdateAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Kategoriyi siler (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
