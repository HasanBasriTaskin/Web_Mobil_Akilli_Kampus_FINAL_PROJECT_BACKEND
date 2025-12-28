using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.FoodItem;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class FoodItemsController : ControllerBase
    {
        private readonly IFoodItemService _foodItemService;

        public FoodItemsController(IFoodItemService foodItemService)
        {
            _foodItemService = foodItemService;
        }

        /// <summary>
        /// Tüm yemek içeriklerini getirir
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            var isAdmin = User.IsInRole("Admin");
            var result = await _foodItemService.GetAllAsync(isAdmin && includeInactive);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Kategoriye göre yemek içeriklerini getirir
        /// </summary>
        [HttpGet("category/{category}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(MealItemCategory category)
        {
            var result = await _foodItemService.GetByCategoryAsync(category);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// ID'ye göre yemek içeriği detayını getirir
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _foodItemService.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Yeni yemek içeriği oluşturur
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FoodItemCreateDto dto)
        {
            var result = await _foodItemService.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Yemek içeriğini günceller
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FoodItemUpdateDto dto)
        {
            var result = await _foodItemService.UpdateAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Yemek içeriğini siler (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _foodItemService.DeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
