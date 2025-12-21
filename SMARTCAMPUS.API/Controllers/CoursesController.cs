using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Course;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        /// <summary>
        /// Tüm dersleri listele (sayfalama ve filtreleme ile)
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllCourses(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? departmentId = null,
            [FromQuery] string? search = null)
        {
            var result = await _courseService.GetAllCoursesAsync(page, pageSize, departmentId, search);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Ders detayını getir
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var result = await _courseService.GetCourseByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Dersin önkoşullarını getir
        /// </summary>
        [HttpGet("{id}/prerequisites")]
        [Authorize]
        public async Task<IActionResult> GetPrerequisites(int id)
        {
            var result = await _courseService.GetPrerequisitesAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Yeni ders oluştur (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
        {
            var result = await _courseService.CreateCourseAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Ders güncelle (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
        {
            var result = await _courseService.UpdateCourseAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Ders sil (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var result = await _courseService.DeleteCourseAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
