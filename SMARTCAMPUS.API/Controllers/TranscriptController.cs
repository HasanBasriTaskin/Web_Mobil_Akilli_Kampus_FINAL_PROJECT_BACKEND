using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class TranscriptController : ControllerBase
    {
        private readonly ITranscriptService _transcriptService;

        public TranscriptController(ITranscriptService transcriptService)
        {
            _transcriptService = transcriptService;
        }

        [HttpGet("students/{studentId}")]
        public async Task<IActionResult> GetTranscript(int studentId)
        {
            var result = await _transcriptService.GetTranscriptAsync(studentId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("students/{studentId}/pdf")]
        public async Task<IActionResult> DownloadTranscriptPdf(int studentId)
        {
            var result = await _transcriptService.GenerateTranscriptPdfAsync(studentId);
            if (!result.IsSuccessful || result.Data == null)
                return StatusCode(result.StatusCode, result);

            return File(result.Data, "application/pdf", $"transcript_{studentId}_{DateTime.UtcNow:yyyyMMdd}.pdf");
        }
    }
}

