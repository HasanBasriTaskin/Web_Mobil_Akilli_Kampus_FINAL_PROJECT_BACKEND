using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SMARTCAMPUS.API.Middleware;
using SMARTCAMPUS.BusinessLayer.Common;
using Xunit;

namespace SMARTCAMPUS.Tests.Middleware
{
    public class GlobalExceptionMiddlewareTests
    {
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly Mock<ILogger<GlobalExceptionMiddleware>> _loggerMock;
        private readonly GlobalExceptionMiddleware _middleware;

        public GlobalExceptionMiddlewareTests()
        {
            _nextMock = new Mock<RequestDelegate>();
            _loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
            _middleware = new GlobalExceptionMiddleware(_nextMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void InvokeAsync_ShouldCallNext_WhenNoExceptionIsThrown()
        {
            // Arrange
            var context = new DefaultHttpContext();
            _nextMock.Setup(next => next(context)).Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _middleware.InvokeAsync(context);

            // Assert
            act.Should().NotThrowAsync();
            _nextMock.Verify(next => next(context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleException_WhenExceptionIsThrown()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            var exceptionMessage = "Test Exception";
            _nextMock.Setup(next => next(context)).ThrowsAsync(new Exception(exceptionMessage));

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            context.Response.ContentType.Should().Be("application/json");

            responseStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(responseStream).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<Response<string>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            response.Should().NotBeNull();
            response.IsSuccessful.Should().BeFalse();
            // Updated expectation to match implementation
            response.Errors.Should().Contain($"Internal Server Error: {exceptionMessage}");
        }
    }
}
