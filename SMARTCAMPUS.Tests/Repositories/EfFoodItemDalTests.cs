using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Repositories
{
    public class EfFoodItemDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfFoodItemDal _dal;

        public EfFoodItemDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfFoodItemDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task NameExistsAsync_ShouldReturnTrue_WhenExists()
        {
            // Arrange
            _context.FoodItems.Add(new FoodItem { Name = "Burger" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.NameExistsAsync("Burger");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetByCategoryAsync_ShouldReturnOnlyActiveItems_WithMatchingCategory()
        {
            // Arrange
            var category1 = MealItemCategory.MainCourse;
            var category2 = MealItemCategory.Dessert;

            _context.FoodItems.AddRange(
                new FoodItem { Name = "Pizza", Category = category1, IsActive = true },
                new FoodItem { Name = "Pasta", Category = category1, IsActive = true },
                new FoodItem { Name = "Cake", Category = category2, IsActive = true },
                new FoodItem { Name = "Inactive Pizza", Category = category1, IsActive = false }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByCategoryAsync(category1);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(f => f.Category == category1 && f.IsActive);
            result.Should().Contain(f => f.Name == "Pizza");
            result.Should().Contain(f => f.Name == "Pasta");
        }

        [Fact]
        public async Task GetByCategoryAsync_ShouldReturnEmptyList_WhenNoActiveItemsInCategory()
        {
            // Arrange
            var category = MealItemCategory.MainCourse;
            _context.FoodItems.Add(new FoodItem { Name = "Pizza", Category = category, IsActive = false });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByCategoryAsync(category);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByCategoryAsync_ShouldReturnEmptyList_WhenCategoryDoesNotExist()
        {
            // Arrange
            var existingCategory = MealItemCategory.MainCourse;
            var nonExistingCategory = MealItemCategory.Dessert;

            _context.FoodItems.Add(new FoodItem { Name = "Pizza", Category = existingCategory, IsActive = true });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByCategoryAsync(nonExistingCategory);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task IsUsedInActiveMenuAsync_ShouldReturnTrue_WhenFoodItemIsInActiveMenu()
        {
            // Arrange
            var cafeteria = new Cafeteria { Id = 1, Name = "Main Cafeteria", Location = "Building A", Capacity = 100, IsActive = true };
            var foodItem = new FoodItem { Id = 1, Name = "Pizza", IsActive = true };
            var menu = new MealMenu { Id = 1, CafeteriaId = 1, Cafeteria = cafeteria, IsActive = true };
            var menuItem = new MealMenuItem { Id = 1, MenuId = 1, FoodItemId = 1, Menu = menu, FoodItem = foodItem };

            _context.Cafeterias.Add(cafeteria);
            _context.FoodItems.Add(foodItem);
            _context.MealMenus.Add(menu);
            _context.MealMenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.IsUsedInActiveMenuAsync(1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsUsedInActiveMenuAsync_ShouldReturnFalse_WhenFoodItemIsInInactiveMenu()
        {
            // Arrange
            var cafeteria = new Cafeteria { Id = 1, Name = "Main Cafeteria", Location = "Building A", Capacity = 100, IsActive = true };
            var foodItem = new FoodItem { Id = 1, Name = "Pizza", IsActive = true };
            var menu = new MealMenu { Id = 1, CafeteriaId = 1, Cafeteria = cafeteria, IsActive = false };
            var menuItem = new MealMenuItem { Id = 1, MenuId = 1, FoodItemId = 1, Menu = menu, FoodItem = foodItem };

            _context.Cafeterias.Add(cafeteria);
            _context.FoodItems.Add(foodItem);
            _context.MealMenus.Add(menu);
            _context.MealMenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.IsUsedInActiveMenuAsync(1);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsUsedInActiveMenuAsync_ShouldReturnFalse_WhenFoodItemNotInAnyMenu()
        {
            // Arrange
            var foodItem = new FoodItem { Id = 1, Name = "Pizza", IsActive = true };
            _context.FoodItems.Add(foodItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.IsUsedInActiveMenuAsync(1);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsUsedInActiveMenuAsync_ShouldReturnFalse_WhenFoodItemIdDoesNotExist()
        {
            // Act
            var result = await _dal.IsUsedInActiveMenuAsync(999);

            // Assert
            result.Should().BeFalse();
        }
    }
}
