using FluentAssertions;
using Moq;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Menu;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using SMARTCAMPUS.Tests.TestUtilities;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class MealMenuManagerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMealMenuDal> _mockMenuDal;
        private readonly Mock<ICafeteriaDal> _mockCafeteriaDal;
        private readonly Mock<IFoodItemDal> _mockFoodItemDal;
        private readonly Mock<IMealMenuItemDal> _mockMenuItemDal;
        private readonly Mock<IMealNutritionDal> _mockNutritionDal;
        private readonly MealMenuManager _manager;

        public MealMenuManagerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMenuDal = new Mock<IMealMenuDal>();
            _mockCafeteriaDal = new Mock<ICafeteriaDal>();
            _mockFoodItemDal = new Mock<IFoodItemDal>();
            _mockMenuItemDal = new Mock<IMealMenuItemDal>();
            _mockNutritionDal = new Mock<IMealNutritionDal>();
            _mockUnitOfWork.Setup(u => u.MealMenus).Returns(_mockMenuDal.Object);
            _mockUnitOfWork.Setup(u => u.Cafeterias).Returns(_mockCafeteriaDal.Object);
            _mockUnitOfWork.Setup(u => u.FoodItems).Returns(_mockFoodItemDal.Object);
            _mockUnitOfWork.Setup(u => u.MealMenuItems).Returns(_mockMenuItemDal.Object);
            _mockUnitOfWork.Setup(u => u.MealNutritions).Returns(_mockNutritionDal.Object);
            _manager = new MealMenuManager(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetMenusAsync_ShouldReturnSuccess()
        {
            var menus = new List<MealMenu>
            {
                new MealMenu
                {
                    Id = 1,
                    CafeteriaId = 1,
                    Cafeteria = new Cafeteria { Name = "Cafeteria1" },
                    MenuItems = new List<MealMenuItem>()
                }
            };
            _mockMenuDal.Setup(x => x.GetMenusAsync(null, null, null)).ReturnsAsync(menus);

            var result = await _manager.GetMenusAsync();

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetMenuByIdAsync_ShouldReturnSuccess_WhenExists()
        {
            var menu = new MealMenu
            {
                Id = 1,
                CafeteriaId = 1,
                Cafeteria = new Cafeteria { Name = "Cafeteria1" },
                MenuItems = new List<MealMenuItem>()
            };
            _mockMenuDal.Setup(x => x.GetByIdWithDetailsAsync(1)).ReturnsAsync(menu);

            var result = await _manager.GetMenuByIdAsync(1);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetMenuByIdAsync_ShouldReturnFail_WhenNotFound()
        {
            _mockMenuDal.Setup(x => x.GetByIdWithDetailsAsync(1)).ReturnsAsync((MealMenu)null!);

            var result = await _manager.GetMenuByIdAsync(1);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task CreateMenuAsync_ShouldReturnSuccess_WhenValid()
        {
            var dto = new MealMenuCreateDto
            {
                CafeteriaId = 1,
                Date = DateTime.Today,
                MealType = MealType.Lunch,
                Price = 50m,
                FoodItemIds = new List<int> { 1, 2 }
            };
            var cafeteria = new Cafeteria { Id = 1, IsActive = true };
            var foodItems = new List<FoodItem>
            {
                new FoodItem { Id = 1, IsActive = true },
                new FoodItem { Id = 2, IsActive = true }
            };
            _mockMenuDal.Setup(x => x.ExistsForCafeteriaDateMealTypeAsync(1, dto.Date, dto.MealType, null))
                .ReturnsAsync(false);
            _mockCafeteriaDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cafeteria);
            var asyncEnumerable = new TestAsyncEnumerable<FoodItem>(foodItems);
            _mockFoodItemDal.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<System.Func<FoodItem, bool>>>()))
                .Returns(asyncEnumerable);
            _mockMenuDal.Setup(x => x.AddAsync(It.IsAny<MealMenu>())).Returns(Task.CompletedTask);
            _mockMenuItemDal.Setup(x => x.AddAsync(It.IsAny<MealMenuItem>())).Returns(Task.CompletedTask);
            _mockMenuDal.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new MealMenu
                {
                    Id = id,
                    CafeteriaId = 1,
                    Cafeteria = cafeteria,
                    MenuItems = new List<MealMenuItem>()
                });
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.CreateMenuAsync(dto);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task CreateMenuAsync_ShouldReturnFail_WhenExists()
        {
            var dto = new MealMenuCreateDto
            {
                CafeteriaId = 1,
                Date = DateTime.Today,
                MealType = MealType.Lunch
            };
            _mockMenuDal.Setup(x => x.ExistsForCafeteriaDateMealTypeAsync(1, dto.Date, dto.MealType, null))
                .ReturnsAsync(true);

            var result = await _manager.CreateMenuAsync(dto);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task PublishMenuAsync_ShouldReturnSuccess_WhenValid()
        {
            var menu = new MealMenu { Id = 1, IsPublished = false };
            _mockMenuDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(menu);
            _mockMenuDal.Setup(x => x.Update(It.IsAny<MealMenu>()));
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.PublishMenuAsync(1);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateMenuAsync_ShouldReturnSuccess()
        {
            var cafeteria = new Cafeteria { Id = 1, IsActive = true };
            var menu = new MealMenu { Id = 1, CafeteriaId = 1, Cafeteria = cafeteria, Date = DateTime.Today, MealType = MealType.Lunch, MenuItems = new List<MealMenuItem>() };
            var dto = new MealMenuCreateDto { CafeteriaId = 1, Date = DateTime.Today, MealType = MealType.Lunch, Price = 60m, FoodItemIds = new List<int>() };
            _mockMenuDal.Setup(x => x.GetByIdWithDetailsAsync(1)).ReturnsAsync(menu);
            _mockMenuDal.Setup(x => x.ExistsForCafeteriaDateMealTypeAsync(1, dto.Date, dto.MealType, 1)).ReturnsAsync(false);
            _mockCafeteriaDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cafeteria);
            _mockMenuItemDal.Setup(x => x.RemoveRange(It.IsAny<IEnumerable<MealMenuItem>>()));
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.UpdateMenuAsync(1, dto);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task UnpublishMenuAsync_ShouldReturnSuccess()
        {
            var menu = new MealMenu { Id = 1, IsPublished = true };
            _mockMenuDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(menu);
            _mockMenuDal.Setup(x => x.Update(It.IsAny<MealMenu>()));
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.UnpublishMenuAsync(1);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteMenuAsync_ShouldReturnSuccess()
        {
            var menu = new MealMenu { Id = 1, MenuItems = new List<MealMenuItem>() };
            _mockMenuDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(menu);
            _mockMenuDal.Setup(x => x.HasActiveReservationsAsync(1)).ReturnsAsync(false);
            _mockMenuDal.Setup(x => x.Update(It.IsAny<MealMenu>()));
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.DeleteMenuAsync(1);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task AddFoodItemToMenuAsync_ShouldReturnSuccess()
        {
            var menu = new MealMenu { Id = 1, MenuItems = new List<MealMenuItem>() };
            var foodItem = new FoodItem { Id = 1, IsActive = true };
            _mockMenuDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(menu);
            _mockFoodItemDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(foodItem);
            _mockMenuItemDal.Setup(x => x.ExistsAsync(1, 1)).ReturnsAsync(false);
            _mockMenuItemDal.Setup(x => x.GetMaxOrderIndexAsync(1)).ReturnsAsync(0);
            _mockMenuItemDal.Setup(x => x.AddAsync(It.IsAny<MealMenuItem>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.AddFoodItemToMenuAsync(1, 1);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task RemoveFoodItemFromMenuAsync_ShouldReturnSuccess()
        {
            var menuItem = new MealMenuItem { Id = 1, MenuId = 1, FoodItemId = 1 };
            _mockMenuItemDal.Setup(x => x.GetByMenuAndFoodItemAsync(1, 1)).ReturnsAsync(menuItem);
            _mockMenuItemDal.Setup(x => x.Remove(It.IsAny<MealMenuItem>()));
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.RemoveFoodItemFromMenuAsync(1, 1);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task GetMenusAsync_ShouldFilterByDate()
        {
            var cafeteria = new Cafeteria { Id = 1, Name = "Main", IsActive = true };
            var menus = new List<MealMenu> { new MealMenu { Id = 1, CafeteriaId = 1, Cafeteria = cafeteria, MenuItems = new List<MealMenuItem>() } };
            _mockMenuDal.Setup(x => x.GetMenusAsync(It.IsAny<DateTime?>(), null, null)).ReturnsAsync(menus);

            var result = await _manager.GetMenusAsync(DateTime.Today);

            result.IsSuccessful.Should().BeTrue();
        }
    }
}

