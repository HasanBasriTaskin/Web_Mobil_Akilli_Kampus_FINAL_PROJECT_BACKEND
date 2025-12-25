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
    }
}
