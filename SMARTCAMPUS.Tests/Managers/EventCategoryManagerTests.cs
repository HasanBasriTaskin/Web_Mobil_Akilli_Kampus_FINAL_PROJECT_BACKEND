using FluentAssertions;
using Moq;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Event;
using SMARTCAMPUS.EntityLayer.Models;
using SMARTCAMPUS.Tests.TestUtilities;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class EventCategoryManagerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IEventCategoryDal> _mockEventCategoryDal;
        private readonly EventCategoryManager _manager;

        public EventCategoryManagerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockEventCategoryDal = new Mock<IEventCategoryDal>();
            _mockUnitOfWork.Setup(u => u.EventCategories).Returns(_mockEventCategoryDal.Object);
            _manager = new EventCategoryManager(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnSuccess_WithActiveCategories()
        {
            var categories = new List<EventCategory>
            {
                new EventCategory { Id = 1, Name = "Category1", IsActive = true },
                new EventCategory { Id = 2, Name = "Category2", IsActive = true }
            };

            var asyncEnumerable = new TestAsyncEnumerable<EventCategory>(categories);
            _mockEventCategoryDal.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<System.Func<EventCategory, bool>>>()))
                .Returns(asyncEnumerable);

            var result = await _manager.GetAllAsync();

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnSuccess_WhenExists()
        {
            var category = new EventCategory { Id = 1, Name = "Category1", IsActive = true };
            _mockEventCategoryDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(category);

            var result = await _manager.GetByIdAsync(1);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Name.Should().Be("Category1");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnFail_WhenNotFound()
        {
            _mockEventCategoryDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((EventCategory)null!);

            var result = await _manager.GetByIdAsync(1);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnSuccess_WhenValid()
        {
            var dto = new EventCategoryCreateDto { Name = "New Category" };
            _mockEventCategoryDal.Setup(x => x.NameExistsAsync("New Category", null)).ReturnsAsync(false);
            _mockEventCategoryDal.Setup(x => x.AddAsync(It.IsAny<EventCategory>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.CreateAsync(dto);

            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnFail_WhenNameExists()
        {
            var dto = new EventCategoryCreateDto { Name = "Existing" };
            _mockEventCategoryDal.Setup(x => x.NameExistsAsync("Existing", null)).ReturnsAsync(true);

            var result = await _manager.CreateAsync(dto);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnSuccess_WhenValid()
        {
            var category = new EventCategory { Id = 1, Name = "Old", IsActive = true };
            var dto = new EventCategoryUpdateDto { Name = "New" };
            _mockEventCategoryDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(category);
            _mockEventCategoryDal.Setup(x => x.NameExistsAsync("New", 1)).ReturnsAsync(false);
            _mockEventCategoryDal.Setup(x => x.Update(It.IsAny<EventCategory>()));
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.UpdateAsync(1, dto);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnSuccess_WhenNoActiveEvents()
        {
            var category = new EventCategory { Id = 1, Name = "Category", IsActive = true };
            _mockEventCategoryDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(category);
            _mockEventCategoryDal.Setup(x => x.HasActiveEventsAsync(1)).ReturnsAsync(false);
            _mockEventCategoryDal.Setup(x => x.Update(It.IsAny<EventCategory>()));
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.DeleteAsync(1);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFail_WhenHasActiveEvents()
        {
            var category = new EventCategory { Id = 1, Name = "Category", IsActive = true };
            _mockEventCategoryDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(category);
            _mockEventCategoryDal.Setup(x => x.HasActiveEventsAsync(1)).ReturnsAsync(true);

            var result = await _manager.DeleteAsync(1);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }
    }
}

