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
    public class EfMealNutritionDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfMealNutritionDal _dal;

        public EfMealNutritionDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfMealNutritionDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetByMenuIdAsync_ShouldReturnNutrition()
        {
            // Arrange
            _context.MealNutritions.Add(new MealNutrition { MenuId = 1, Calories = 500 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByMenuIdAsync(1);

            // Assert
            result.Should().NotBeNull();
        }
    }
}
