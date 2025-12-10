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

        [Fact]
        public void PagedResponse_HasPrevious_ShouldBeTrue_WhenNotOnFirstPage()
        {
            // Arrange
            var data = new List<string> { "Item1" };

            // Act
            var response = new PagedResponse<string>(data, 2, 10, 20);

            // Assert
            response.HasPrevious.Should().BeTrue();
            response.HasNext.Should().BeFalse();
        }

        [Fact]
        public void PagedResponse_BothNavigations_ShouldBeTrue_WhenOnMiddlePage()
        {
            // Arrange
            var data = new List<string> { "Item1" };

            // Act
            var response = new PagedResponse<string>(data, 2, 10, 30);

            // Assert
            response.HasPrevious.Should().BeTrue();
            response.HasNext.Should().BeTrue();
        }

        [Fact]
        public void PagedResponse_BothNavigations_ShouldBeFalse_WhenSinglePage()
        {
            // Arrange
            var data = new List<string> { "Item1" };

            // Act
            var response = new PagedResponse<string>(data, 1, 10, 5);

            // Assert
            response.HasPrevious.Should().BeFalse();
            response.HasNext.Should().BeFalse();
            response.TotalPages.Should().Be(1);
        }

        [Fact]
        public void PagedResponse_TotalPages_ShouldBeZero_WhenNoRecords()
        {
            // Arrange
            var data = new List<string>();

            // Act
            var response = new PagedResponse<string>(data, 1, 10, 0);

            // Assert
            response.TotalRecords.Should().Be(0);
            response.TotalPages.Should().Be(0);
            response.HasNext.Should().BeFalse();
            response.HasPrevious.Should().BeFalse();
        }

        [Fact]
        public void PagedResponse_TotalPages_ShouldRoundUpCorrectly()
        {
            // Arrange
            var data = new List<string> { "Item1" };

            // Act - 15 records with 10 per page should give 2 pages
            var response = new PagedResponse<string>(data, 1, 10, 15);

            // Assert
            response.TotalPages.Should().Be(2);
        }

        [Fact]
        public void PagedResponse_ShouldWorkWithEmptyData()
        {
            // Arrange
            var data = new List<string>();

            // Act
            var response = new PagedResponse<string>(data, 1, 10, 0);

            // Assert
            response.Data.Should().BeEmpty();
            response.TotalRecords.Should().Be(0);
        }
    }
}

