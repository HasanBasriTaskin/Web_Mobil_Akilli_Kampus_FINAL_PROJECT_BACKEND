using FluentAssertions;
using SMARTCAMPUS.API.Controllers;
using Xunit;

namespace SMARTCAMPUS.Tests.DTOs
{
    public class EventCancelDtoTests
    {
        [Fact]
        public void EventCancelDto_ShouldHaveReasonProperty()
        {
            var dto = new EventCancelDto
            {
                Reason = "Test Reason"
            };

            dto.Reason.Should().Be("Test Reason");
        }

        [Fact]
        public void EventCancelDto_ShouldAllowNullReason()
        {
            var dto = new EventCancelDto
            {
                Reason = null!
            };

            dto.Reason.Should().BeNull();
        }
    }
}

