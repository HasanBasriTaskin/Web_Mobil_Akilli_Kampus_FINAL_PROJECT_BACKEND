using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class SectionsController : ControllerBase
    {
        private readonly ICourseSectionService _sectionService;

        public SectionsController(ICourseSectionService sectionService)
        {
            _sectionService = sectionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSections([FromQuery] CourseSectionQueryParameters queryParams)
        {
            var result = await _sectionService.GetSectionsAsync(queryParams);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSection(int id)
        {
            var result = await _sectionService.GetSectionByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSection([FromBody] CourseSectionCreateDto sectionCreateDto)
        {
            var result = await _sectionService.CreateSectionAsync(sectionCreateDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSection(int id, [FromBody] CourseSectionUpdateDto sectionUpdateDto)
        {
            var result = await _sectionService.UpdateSectionAsync(id, sectionUpdateDto);
            return StatusCode(result.StatusCode, result);
        }
    }
}

