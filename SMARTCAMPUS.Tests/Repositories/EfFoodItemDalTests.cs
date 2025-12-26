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
    }
}
