using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses([FromQuery] CourseQueryParameters queryParams)
        {
            var result = await _courseService.GetCoursesAsync(queryParams);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            var result = await _courseService.GetCourseByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetCourseByCode(string code)
        {
            var result = await _courseService.GetCourseByCodeAsync(code);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> GetCoursesByDepartment(int departmentId)
        {
            var result = await _courseService.GetCoursesByDepartmentAsync(departmentId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{courseId}/sections")]
        public async Task<IActionResult> GetCourseSections(int courseId)
        {
            var result = await _courseService.GetCourseSectionsAsync(courseId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("sections/{sectionId}")]
        public async Task<IActionResult> GetSection(int sectionId)
        {
            var result = await _courseService.GetSectionByIdAsync(sectionId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseCreateDto courseCreateDto)
        {
            var result = await _courseService.CreateCourseAsync(courseCreateDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseUpdateDto courseUpdateDto)
        {
            var result = await _courseService.UpdateCourseAsync(id, courseUpdateDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var result = await _courseService.DeleteCourseAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}



