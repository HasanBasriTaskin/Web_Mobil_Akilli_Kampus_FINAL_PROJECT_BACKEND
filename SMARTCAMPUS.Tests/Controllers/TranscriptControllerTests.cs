using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class TranscriptControllerTests
    {
        private readonly Mock<ITranscriptService> _mockService;
        private readonly TranscriptController _controller;

        public TranscriptControllerTests()
        {
            _mockService = new Mock<ITranscriptService>();
            _controller = new TranscriptController(_mockService.Object);
        }

        #region GetTranscript Tests

        [Fact]
        public async Task GetTranscript_ShouldReturnStatusCode_WhenFound()
        {
            // Arrange
            var transcript = new TranscriptDto
            {
                StudentId = 1,
                StudentName = "John Doe"
            };
            var response = Response<TranscriptDto>.Success(transcript, 200);
            _mockService.Setup(x => x.GetTranscriptAsync(1)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetTranscript(1) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            _mockService.Verify(x => x.GetTranscriptAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetTranscript_ShouldReturnNotFound_WhenStudentNotExists()
        {
            // Arrange
            var response = Response<TranscriptDto>.Fail("Student not found", 404);
            _mockService.Setup(x => x.GetTranscriptAsync(999)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetTranscript(999) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(404);
        }

        #endregion

        #region DownloadTranscriptPdf Tests

        [Fact]
        public async Task DownloadTranscriptPdf_ShouldReturnFile_WhenSuccess()
        {
            // Arrange
            var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF magic bytes
            var response = Response<byte[]>.Success(pdfBytes, 200);
            _mockService.Setup(x => x.GenerateTranscriptPdfAsync(1)).ReturnsAsync(response);

            // Act
            var result = await _controller.DownloadTranscriptPdf(1) as FileContentResult;

            // Assert
            result.Should().NotBeNull();
            result!.ContentType.Should().Be("application/pdf");
            result.FileContents.Should().BeEquivalentTo(pdfBytes);
            result.FileDownloadName.Should().StartWith("transcript_1_");
        }

        [Fact]
        public async Task DownloadTranscriptPdf_ShouldReturnError_WhenNotSuccessful()
        {
            // Arrange
            var response = Response<byte[]>.Fail("Student not found", 404);
            _mockService.Setup(x => x.GenerateTranscriptPdfAsync(999)).ReturnsAsync(response);

            // Act
            var result = await _controller.DownloadTranscriptPdf(999) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task DownloadTranscriptPdf_ShouldReturnError_WhenDataIsNull()
        {
            // Arrange
            var response = Response<byte[]>.Success(null!, 200);
            _mockService.Setup(x => x.GenerateTranscriptPdfAsync(1)).ReturnsAsync(response);

            // Act
            var result = await _controller.DownloadTranscriptPdf(1) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        #endregion
    }
}
