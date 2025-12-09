using FluentAssertions;
using SMARTCAMPUS.BusinessLayer.Common;
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
    }
}
