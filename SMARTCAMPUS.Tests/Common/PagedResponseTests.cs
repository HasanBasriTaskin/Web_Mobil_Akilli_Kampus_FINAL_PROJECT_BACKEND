using FluentAssertions;
using SMARTCAMPUS.BusinessLayer.Common;
using Xunit;

namespace SMARTCAMPUS.Tests.Common
{
    public class PagedResponseTests
    {
        [Fact]
        public void PagedResponse_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var data = new List<string> { "Item1", "Item2" };
            var pageNumber = 1;
            var pageSize = 10;
            var totalRecords = 20;

            // Act
            var response = new PagedResponse<string>(data, pageNumber, pageSize, totalRecords);

            // Assert
            response.Data.Should().BeEquivalentTo(data);
            response.PageNumber.Should().Be(pageNumber);
            response.PageSize.Should().Be(pageSize);
            response.TotalRecords.Should().Be(totalRecords);
            response.TotalPages.Should().Be(2);
            response.HasNext.Should().BeTrue();
            response.HasPrevious.Should().BeFalse();
        }
    }
}
