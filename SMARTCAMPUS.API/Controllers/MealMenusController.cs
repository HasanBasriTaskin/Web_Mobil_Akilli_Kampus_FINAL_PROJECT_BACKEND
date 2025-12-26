using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Menu;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MealMenusController : ControllerBase
    {
        private readonly IMealMenuService _menuService;

        public MealMenusController(IMealMenuService menuService)
        {
            _menuService = menuService;
        }

        /// <summary>
        /// Menüleri filtreli getirir
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetMenus(
            [FromQuery] DateTime? date,
            [FromQuery] int? cafeteriaId,
            [FromQuery] MealType? mealType)
        {
            var result = await _menuService.GetMenusAsync(date, cafeteriaId, mealType);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Menü detayını getirir
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _menuService.GetMenuByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Yeni menü oluşturur
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] MealMenuCreateDto dto)
        {
            var result = await _menuService.CreateMenuAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Menüyü günceller
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] MealMenuCreateDto dto)
        {
            var result = await _menuService.UpdateMenuAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Menüyü siler (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool force = false)
        {
            var result = await _menuService.DeleteMenuAsync(id, force);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Menüyü yayınlar
        /// </summary>
        [HttpPut("{id}/publish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Publish(int id)
        {
            var result = await _menuService.PublishMenuAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Menü yayınını geri çeker
        /// </summary>
        [HttpPut("{id}/unpublish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unpublish(int id)
        {
            var result = await _menuService.UnpublishMenuAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Menüye yemek içeriği ekler
        /// </summary>
        [HttpPost("{menuId}/items/{foodItemId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddFoodItem(int menuId, int foodItemId)
        {
            var result = await _menuService.AddFoodItemToMenuAsync(menuId, foodItemId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Menüden yemek içeriği çıkarır
        /// </summary>
        [HttpDelete("{menuId}/items/{foodItemId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveFoodItem(int menuId, int foodItemId)
        {
            var result = await _menuService.RemoveFoodItemFromMenuAsync(menuId, foodItemId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
