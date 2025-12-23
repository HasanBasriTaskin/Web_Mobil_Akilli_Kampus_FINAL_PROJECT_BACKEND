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
    public class EfCafeteriaDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfCafeteriaDal _dal;

        public EfCafeteriaDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfCafeteriaDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task NameExistsAsync_ShouldReturnTrue_WhenNameExists()
        {
            // Arrange
            _context.Cafeterias.Add(new Cafeteria { Name = "Main Cafeteria" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.NameExistsAsync("Main Cafeteria");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveMenusAsync_ShouldReturnTrue_WhenActiveMenuExists()
        {
            // Arrange
            var cafe = new Cafeteria { Name = "Main" };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            _context.MealMenus.Add(new MealMenu { CafeteriaId = cafe.Id, IsActive = true, Date = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.HasActiveMenusAsync(cafe.Id);

            // Assert
            result.Should().BeTrue();
        }
    }
}
