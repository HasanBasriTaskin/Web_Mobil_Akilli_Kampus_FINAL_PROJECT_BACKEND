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
            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            var date = DateTime.UtcNow.Date;
            var m1 = new MealMenu { Date = date, CafeteriaId = cafe.Id, MealType = MealType.Lunch, IsActive = true, IsPublished = true };
            _context.MealMenus.Add(m1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetMenusAsync(date, cafe.Id, null);

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetMenusAsync_ShouldFilterByMealType()
        {
            // Arrange
            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            var date = DateTime.UtcNow.Date;
            var lunch = new MealMenu { Date = date, CafeteriaId = cafe.Id, MealType = MealType.Lunch, IsActive = true, IsPublished = true };
            var dinner = new MealMenu { Date = date, CafeteriaId = cafe.Id, MealType = MealType.Dinner, IsActive = true, IsPublished = true };
            _context.MealMenus.AddRange(lunch, dinner);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetMenusAsync(date, cafe.Id, MealType.Lunch);

            // Assert
            result.Should().HaveCount(1);
            result.First().MealType.Should().Be(MealType.Lunch);
        }

        [Fact]
        public async Task GetMenusAsync_ShouldOnlyReturnPublishedAndActive()
        {
            // Arrange
            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            var date = DateTime.UtcNow.Date;
            var published = new MealMenu { Date = date, CafeteriaId = cafe.Id, MealType = MealType.Lunch, IsActive = true, IsPublished = true };
            var unpublished = new MealMenu { Date = date, CafeteriaId = cafe.Id, MealType = MealType.Dinner, IsActive = true, IsPublished = false };
            var inactive = new MealMenu { Date = date, CafeteriaId = cafe.Id, MealType = MealType.Breakfast, IsActive = false, IsPublished = true };
            _context.MealMenus.AddRange(published, unpublished, inactive);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetMenusAsync(date, cafe.Id, null);

            // Assert
            result.Should().HaveCount(1);
            result.First().Should().Be(published);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_ShouldIncludeRelations()
        {
            // Arrange
            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            var menu = new MealMenu 
            { 
                Date = DateTime.UtcNow.Date, 
                CafeteriaId = cafe.Id, 
                MealType = MealType.Lunch, 
                IsActive = true, 
                IsPublished = true 
            };
            _context.MealMenus.Add(menu);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByIdWithDetailsAsync(menu.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Cafeteria.Should().NotBeNull();
        }

        [Fact]
        public async Task ExistsForCafeteriaDateMealTypeAsync_ShouldReturnTrue_WhenExists()
        {
            // Arrange
            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            var date = DateTime.UtcNow.Date;
            var menu = new MealMenu 
            { 
                Date = date, 
                CafeteriaId = cafe.Id, 
                MealType = MealType.Lunch, 
                IsActive = true 
            };
            _context.MealMenus.Add(menu);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.ExistsForCafeteriaDateMealTypeAsync(cafe.Id, date, MealType.Lunch);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsForCafeteriaDateMealTypeAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Arrange
            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.ExistsForCafeteriaDateMealTypeAsync(cafe.Id, DateTime.UtcNow.Date, MealType.Lunch);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsForCafeteriaDateMealTypeAsync_ShouldExcludeId_WhenProvided()
        {
            // Arrange
            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            var date = DateTime.UtcNow.Date;
            var menu1 = new MealMenu { Date = date, CafeteriaId = cafe.Id, MealType = MealType.Lunch, IsActive = true };
            var menu2 = new MealMenu { Date = date, CafeteriaId = cafe.Id, MealType = MealType.Lunch, IsActive = true };
            _context.MealMenus.AddRange(menu1, menu2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.ExistsForCafeteriaDateMealTypeAsync(cafe.Id, date, MealType.Lunch, menu1.Id);

            // Assert
            result.Should().BeTrue(); // menu2 exists
        }

        [Fact]
        public async Task HasActiveReservationsAsync_ShouldReturnTrue_WhenActiveReservationExists()
        {
            // Arrange
            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var menu = new MealMenu { CafeteriaId = cafe.Id, Date = DateTime.UtcNow, MealType = MealType.Lunch, IsActive = true };
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
                QRCode = "TEST-QR"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.HasActiveReservationsAsync(menu.Id);

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

            var menu = new MealMenu { CafeteriaId = cafe.Id, Date = DateTime.UtcNow, MealType = MealType.Lunch, IsActive = true };
            _context.MealMenus.Add(menu);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.HasActiveReservationsAsync(menu.Id);

            // Assert
            result.Should().BeFalse();
        }
    }
}
