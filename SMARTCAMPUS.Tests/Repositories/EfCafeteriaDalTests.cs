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
            _context.Cafeterias.Add(new Cafeteria { Name = "Main Cafeteria", Location = "Building A", Capacity = 100 });
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
            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            _context.MealMenus.Add(new MealMenu { CafeteriaId = cafe.Id, IsActive = true, Date = DateTime.UtcNow, MealType = MealType.Lunch });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.HasActiveMenusAsync(cafe.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveMenusAsync_ShouldReturnFalse_WhenNoActiveMenuExists()
        {
            // Arrange
            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.HasActiveMenusAsync(cafe.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveMenusAsync_ShouldReturnFalse_WhenOnlyInactiveMenuExists()
        {
            // Arrange
            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            _context.MealMenus.Add(new MealMenu { CafeteriaId = cafe.Id, IsActive = false, Date = DateTime.UtcNow, MealType = MealType.Lunch });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.HasActiveMenusAsync(cafe.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task NameExistsAsync_ShouldReturnFalse_WhenNameDoesNotExist()
        {
            // Arrange
            _context.Cafeterias.Add(new Cafeteria { Name = "Main Cafeteria", Location = "Building A", Capacity = 100 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.NameExistsAsync("Non-existent Cafeteria");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task NameExistsAsync_ShouldReturnFalse_WhenExcludingId()
        {
            // Arrange
            var cafe1 = new Cafeteria { Name = "Main Cafeteria", Location = "Building A", Capacity = 100 };
            var cafe2 = new Cafeteria { Name = "Second Cafeteria", Location = "Building B", Capacity = 50 };
            _context.Cafeterias.AddRange(cafe1, cafe2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.NameExistsAsync("Main Cafeteria", cafe1.Id);

            // Assert
            result.Should().BeFalse(); // Should return false because we're excluding cafe1
        }

        [Fact]
        public async Task NameExistsAsync_ShouldReturnTrue_WhenExcludingDifferentId()
        {
            // Arrange
            var cafe1 = new Cafeteria { Name = "Main Cafeteria", Location = "Building A", Capacity = 100 };
            var cafe2 = new Cafeteria { Name = "Main Cafeteria", Location = "Building B", Capacity = 50 };
            _context.Cafeterias.AddRange(cafe1, cafe2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.NameExistsAsync("Main Cafeteria", cafe1.Id);

            // Assert
            result.Should().BeTrue(); // Should return true because cafe2 has the same name
        }

        [Fact]
        public async Task HasActiveReservationsAsync_ShouldReturnTrue_WhenActiveReservationExists()
        {
            // Arrange
            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            var user = new User { Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var menu = new MealMenu { CafeteriaId = cafe.Id, IsActive = true, Date = DateTime.UtcNow, MealType = MealType.Lunch };
            _context.MealMenus.Add(menu);
            await _context.SaveChangesAsync();

            _context.MealReservations.Add(new MealReservation
            {
                UserId = user.Id,
                MenuId = menu.Id,
                CafeteriaId = cafe.Id,
                MealType = MealType.Lunch,
                Date = DateTime.UtcNow,
                Status = MealReservationStatus.Reserved,
                QRCode = "TEST-QR-123"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.HasActiveReservationsAsync(cafe.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveReservationsAsync_ShouldReturnFalse_WhenNoActiveReservationExists()
        {
            // Arrange
            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.HasActiveReservationsAsync(cafe.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveReservationsAsync_ShouldReturnFalse_WhenOnlyUsedReservationExists()
        {
            // Arrange
            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            var user = new User { Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var menu = new MealMenu { CafeteriaId = cafe.Id, IsActive = true, Date = DateTime.UtcNow, MealType = MealType.Lunch };
            _context.MealMenus.Add(menu);
            await _context.SaveChangesAsync();

            _context.MealReservations.Add(new MealReservation
            {
                UserId = user.Id,
                MenuId = menu.Id,
                CafeteriaId = cafe.Id,
                MealType = MealType.Lunch,
                Date = DateTime.UtcNow,
                Status = MealReservationStatus.Used,
                QRCode = "TEST-QR-123"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.HasActiveReservationsAsync(cafe.Id);

            // Assert
            result.Should().BeFalse(); // Used reservations are not considered active
        }
    }
}
