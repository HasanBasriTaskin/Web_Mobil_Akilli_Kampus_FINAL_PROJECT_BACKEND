using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Repositories
{
    public class EfMealMenuItemDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfMealMenuItemDal _dal;

        public EfMealMenuItemDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfMealMenuItemDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenExists()
        {
            // Arrange
            _context.MealMenuItems.Add(new MealMenuItem { MenuId = 1, FoodItemId = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.ExistsAsync(1, 1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Arrange
            _context.MealMenuItems.Add(new MealMenuItem { MenuId = 1, FoodItemId = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.ExistsAsync(1, 2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetMaxOrderIndexAsync_ShouldReturnMaxOrderIndex_WhenItemsExist()
        {
            // Arrange
            _context.MealMenuItems.AddRange(new[]
            {
                new MealMenuItem { MenuId = 1, FoodItemId = 1, OrderIndex = 1 },
                new MealMenuItem { MenuId = 1, FoodItemId = 2, OrderIndex = 5 },
                new MealMenuItem { MenuId = 1, FoodItemId = 3, OrderIndex = 3 },
                new MealMenuItem { MenuId = 2, FoodItemId = 4, OrderIndex = 10 } // Different menu
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetMaxOrderIndexAsync(1);

            // Assert
            result.Should().Be(5);
        }

        [Fact]
        public async Task GetMaxOrderIndexAsync_ShouldReturnNegativeOne_WhenNoItemsExist()
        {
            // Arrange - No items in database

            // Act
            var result = await _dal.GetMaxOrderIndexAsync(1);

            // Assert
            result.Should().Be(-1);
        }

        [Fact]
        public async Task GetByMenuAndFoodItemAsync_ShouldReturnItem_WhenExists()
        {
            // Arrange
            var item = new MealMenuItem { MenuId = 1, FoodItemId = 1, OrderIndex = 1 };
            _context.MealMenuItems.Add(item);
            _context.MealMenuItems.Add(new MealMenuItem { MenuId = 1, FoodItemId = 2, OrderIndex = 2 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByMenuAndFoodItemAsync(1, 1);

            // Assert
            result.Should().NotBeNull();
            result!.MenuId.Should().Be(1);
            result.FoodItemId.Should().Be(1);
        }

        [Fact]
        public async Task GetByMenuAndFoodItemAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            _context.MealMenuItems.Add(new MealMenuItem { MenuId = 1, FoodItemId = 1, OrderIndex = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByMenuAndFoodItemAsync(1, 2);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByMenuAndFoodItemAsync_ShouldReturnNull_WhenMenuIdDoesNotMatch()
        {
            // Arrange
            _context.MealMenuItems.Add(new MealMenuItem { MenuId = 1, FoodItemId = 1, OrderIndex = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByMenuAndFoodItemAsync(2, 1);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RemoveByMenuIdAsync_ShouldRemoveAllItems_ForGivenMenuId()
        {
            // Arrange
            _context.MealMenuItems.AddRange(new[]
            {
                new MealMenuItem { MenuId = 1, FoodItemId = 1, OrderIndex = 1 },
                new MealMenuItem { MenuId = 1, FoodItemId = 2, OrderIndex = 2 },
                new MealMenuItem { MenuId = 2, FoodItemId = 3, OrderIndex = 1 } // Different menu
            });
            await _context.SaveChangesAsync();

            // Act
            await _dal.RemoveByMenuIdAsync(1);
            await _context.SaveChangesAsync();

            // Assert
            var remainingItems = await _context.MealMenuItems.ToListAsync();
            remainingItems.Should().HaveCount(1);
            remainingItems.First().MenuId.Should().Be(2);
        }

        [Fact]
        public async Task RemoveByMenuIdAsync_ShouldNotRemoveItems_ForDifferentMenuId()
        {
            // Arrange
            _context.MealMenuItems.AddRange(new[]
            {
                new MealMenuItem { MenuId = 1, FoodItemId = 1, OrderIndex = 1 },
                new MealMenuItem { MenuId = 2, FoodItemId = 2, OrderIndex = 1 }
            });
            await _context.SaveChangesAsync();

            // Act
            await _dal.RemoveByMenuIdAsync(1);
            await _context.SaveChangesAsync();

            // Assert
            var remainingItems = await _context.MealMenuItems.ToListAsync();
            remainingItems.Should().HaveCount(1);
            remainingItems.First().MenuId.Should().Be(2);
        }

        [Fact]
        public async Task RemoveByMenuIdAsync_ShouldNotThrow_WhenNoItemsExist()
        {
            // Arrange - No items in database

            // Act & Assert
            await _dal.Invoking(d => d.RemoveByMenuIdAsync(1))
                .Should().NotThrowAsync();
        }
    }
}
