using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;

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
        public async Task<IActionResult> GetCourses()
        {
            var result = await _courseService.GetCoursesAsync();
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
    }
}

