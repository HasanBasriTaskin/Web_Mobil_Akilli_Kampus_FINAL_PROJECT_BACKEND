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
    public class EfMealMenuDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfMealMenuDal _dal;

        public EfMealMenuDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfMealMenuDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetMenusAsync_ShouldFilter()
        {
            // Arrange
            var date = DateTime.UtcNow.Date;
            var m1 = new MealMenu { Date = date, CafeteriaId = 1, MealType = MealType.Lunch, IsActive = true, IsPublished = true };
            _context.MealMenus.Add(m1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetMenusAsync(date, 1, null);

            // Assert
            result.Should().HaveCount(1);
        }
    }
}
