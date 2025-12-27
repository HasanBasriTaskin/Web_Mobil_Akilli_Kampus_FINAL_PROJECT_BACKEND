using FluentAssertions;
using Moq;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Cafeteria;
using SMARTCAMPUS.EntityLayer.Models;
using SMARTCAMPUS.Tests.TestUtilities;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class CafeteriaManagerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICafeteriaDal> _mockDal;
        private readonly CafeteriaManager _manager;

        public CafeteriaManagerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockDal = new Mock<ICafeteriaDal>();
            _mockUnitOfWork.Setup(u => u.Cafeterias).Returns(_mockDal.Object);
            _manager = new CafeteriaManager(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnSuccess()
        {
            var cafeterias = new List<Cafeteria>
            {
                new Cafeteria { Id = 1, Name = "Cafeteria1", IsActive = true },
                new Cafeteria { Id = 2, Name = "Cafeteria2", IsActive = true }
            };
            var asyncEnumerable = new TestAsyncEnumerable<Cafeteria>(cafeterias);
            _mockDal.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<System.Func<Cafeteria, bool>>>()))
                .Returns(asyncEnumerable);

            var result = await _manager.GetAllAsync();

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnSuccess_WhenExists()
        {
            var cafeteria = new Cafeteria { Id = 1, Name = "Cafeteria1", IsActive = true };
            _mockDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cafeteria);

            var result = await _manager.GetByIdAsync(1);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Name.Should().Be("Cafeteria1");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnFail_WhenNotFound()
        {
            _mockDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Cafeteria)null!);

            var result = await _manager.GetByIdAsync(1);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnSuccess_WhenValid()
        {
            var dto = new CafeteriaCreateDto { Name = "New", Location = "Location1", Capacity = 100 };
            _mockDal.Setup(x => x.NameExistsAsync("New", null)).ReturnsAsync(false);
            _mockDal.Setup(x => x.AddAsync(It.IsAny<Cafeteria>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.CreateAsync(dto);

            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnFail_WhenNameExists()
        {
            var dto = new CafeteriaCreateDto { Name = "Existing" };
            _mockDal.Setup(x => x.NameExistsAsync("Existing", null)).ReturnsAsync(true);

            var result = await _manager.CreateAsync(dto);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnSuccess_WhenValid()
        {
            var cafeteria = new Cafeteria { Id = 1, Name = "Old", IsActive = true };
            var dto = new CafeteriaUpdateDto { Name = "New" };
            _mockDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cafeteria);
            _mockDal.Setup(x => x.NameExistsAsync("New", 1)).ReturnsAsync(false);
            _mockDal.Setup(x => x.Update(It.IsAny<Cafeteria>()));
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.UpdateAsync(1, dto);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnSuccess_WhenNoActiveMenus()
        {
            var cafeteria = new Cafeteria { Id = 1, Name = "Cafeteria", IsActive = true };
            _mockDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cafeteria);
            _mockDal.Setup(x => x.HasActiveMenusAsync(1)).ReturnsAsync(false);
            _mockDal.Setup(x => x.HasActiveReservationsAsync(1)).ReturnsAsync(false);
            _mockDal.Setup(x => x.Update(It.IsAny<Cafeteria>()));
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.DeleteAsync(1);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFail_WhenHasActiveMenus()
        {
            var cafeteria = new Cafeteria { Id = 1, Name = "Cafeteria", IsActive = true };
            _mockDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cafeteria);
            _mockDal.Setup(x => x.HasActiveMenusAsync(1)).ReturnsAsync(true);

            var result = await _manager.DeleteAsync(1);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }
    }
}

