using FluentAssertions;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using Xunit;

namespace SMARTCAMPUS.Tests.Common
{
    public class ResponseTests
    {
        [Fact]
        public void Response_Success_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var data = "Test Data";

            // Act
            var response = Response<string>.Success(data, 200);

            // Assert
            response.IsSuccessful.Should().BeTrue();
            response.Data.Should().Be(data);
            response.Errors.Should().BeNull();
            response.StatusCode.Should().Be(200);
        }

        [Fact]
        public void Response_Fail_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var message = "Error Message";

            // Act
            var response = Response<string>.Fail(message, 400);

            // Assert
            response.IsSuccessful.Should().BeFalse();
            response.Errors.Should().Contain(message);
            response.Data.Should().BeNull();
            response.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Response_Fail_List_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var messages = new List<string> { "Error1", "Error2" };

            // Act
            var response = Response<string>.Fail(messages, 400);

            // Assert
            response.IsSuccessful.Should().BeFalse();
            response.Errors.Should().BeEquivalentTo(messages);
            response.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Response_Success_WithStatusCodeOnly_ShouldSetPropertiesCorrectly()
        {
            // Act
            var response = Response<NoDataDto>.Success(200);

            // Assert
            response.IsSuccessful.Should().BeTrue();
            response.StatusCode.Should().Be(200);
            response.Data.Should().NotBeNull();
            response.Data.Should().BeOfType<NoDataDto>();
        }

        [Fact]
        public void Response_Success_WithStatusCodeOnly_NonNoDataDto_ShouldHaveNullData()
        {
            // Act
            var response = Response<string>.Success(200);

            // Assert
            response.IsSuccessful.Should().BeTrue();
            response.StatusCode.Should().Be(200);
            response.Data.Should().BeNull();
        }

        [Fact]
        public void Response_Fail_ShouldHandle404StatusCode()
        {
            // Act
            var response = Response<string>.Fail("Not Found", 404);

            // Assert
            response.IsSuccessful.Should().BeFalse();
            response.StatusCode.Should().Be(404);
            response.Errors.Should().Contain("Not Found");
        }

        [Fact]
        public void Response_Fail_ShouldHandle500StatusCode()
        {
            // Act
            var response = Response<string>.Fail("Internal Server Error", 500);

            // Assert
            response.IsSuccessful.Should().BeFalse();
            response.StatusCode.Should().Be(500);
        }
    }
}

