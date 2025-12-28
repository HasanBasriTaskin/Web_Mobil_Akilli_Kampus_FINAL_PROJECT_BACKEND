using FluentAssertions;
using Moq;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.FoodItem;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using SMARTCAMPUS.Tests.TestUtilities;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class FoodItemManagerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IFoodItemDal> _mockFoodItemDal;
        private readonly FoodItemManager _manager;

        public FoodItemManagerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockFoodItemDal = new Mock<IFoodItemDal>();
            _mockUnitOfWork.Setup(u => u.FoodItems).Returns(_mockFoodItemDal.Object);
            _manager = new FoodItemManager(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnSuccess_WithActiveItems()
        {
            var items = new List<FoodItem>
            {
                new FoodItem { Id = 1, Name = "Item1", IsActive = true },
                new FoodItem { Id = 2, Name = "Item2", IsActive = true }
            };

            var asyncEnumerable = new TestAsyncEnumerable<FoodItem>(items);
            _mockFoodItemDal.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<System.Func<FoodItem, bool>>>()))
                .Returns(asyncEnumerable);

            var result = await _manager.GetAllAsync();

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByCategoryAsync_ShouldReturnSuccess()
        {
            var items = new List<FoodItem>
            {
                new FoodItem { Id = 1, Name = "Item1", Category = MealItemCategory.MainCourse }
            };
            _mockFoodItemDal.Setup(x => x.GetByCategoryAsync(MealItemCategory.MainCourse)).ReturnsAsync(items);

            var result = await _manager.GetByCategoryAsync(MealItemCategory.MainCourse);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnSuccess_WhenExists()
        {
            var item = new FoodItem { Id = 1, Name = "Item1", Category = MealItemCategory.MainCourse };
            _mockFoodItemDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(item);

            var result = await _manager.GetByIdAsync(1);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Name.Should().Be("Item1");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnFail_WhenNotFound()
        {
            _mockFoodItemDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((FoodItem)null!);

            var result = await _manager.GetByIdAsync(1);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnSuccess_WhenValid()
        {
            var dto = new FoodItemCreateDto { Name = "New Item", Category = MealItemCategory.MainCourse };
            _mockFoodItemDal.Setup(x => x.NameExistsAsync("New Item", null)).ReturnsAsync(false);
            _mockFoodItemDal.Setup(x => x.AddAsync(It.IsAny<FoodItem>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.CreateAsync(dto);

            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnFail_WhenNameExists()
        {
            var dto = new FoodItemCreateDto { Name = "Existing" };
            _mockFoodItemDal.Setup(x => x.NameExistsAsync("Existing", null)).ReturnsAsync(true);

            var result = await _manager.CreateAsync(dto);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnSuccess_WhenValid()
        {
            var item = new FoodItem { Id = 1, Name = "Old", Category = MealItemCategory.MainCourse };
            var dto = new FoodItemUpdateDto { Name = "New" };
            _mockFoodItemDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(item);
            _mockFoodItemDal.Setup(x => x.NameExistsAsync("New", 1)).ReturnsAsync(false);
            _mockFoodItemDal.Setup(x => x.Update(It.IsAny<FoodItem>()));
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.UpdateAsync(1, dto);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnSuccess_WhenNotUsedInMenu()
        {
            var item = new FoodItem { Id = 1, Name = "Item", IsActive = true };
            _mockFoodItemDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(item);
            _mockFoodItemDal.Setup(x => x.IsUsedInActiveMenuAsync(1)).ReturnsAsync(false);
            _mockFoodItemDal.Setup(x => x.Update(It.IsAny<FoodItem>()));
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.DeleteAsync(1);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFail_WhenUsedInActiveMenu()
        {
            var item = new FoodItem { Id = 1, Name = "Item", IsActive = true };
            _mockFoodItemDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(item);
            _mockFoodItemDal.Setup(x => x.IsUsedInActiveMenuAsync(1)).ReturnsAsync(true);

            var result = await _manager.DeleteAsync(1);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }
    }
}

