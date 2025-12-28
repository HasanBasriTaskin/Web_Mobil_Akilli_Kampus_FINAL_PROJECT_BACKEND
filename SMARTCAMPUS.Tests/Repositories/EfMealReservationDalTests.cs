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
    public class EfMealReservationDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfMealReservationDal _dal;

        public EfMealReservationDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfMealReservationDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetByQRCodeAsync_ShouldReturnReservation()
        {
            // Arrange
            var user = new User 
            { 
                Id = "user1",
                Email = "user1@test.com",
                FullName = "Test User",
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            var menu = new MealMenu { CafeteriaId = cafe.Id, Date = DateTime.UtcNow, MealType = MealType.Lunch, IsActive = true };
            _context.MealMenus.Add(menu);
            await _context.SaveChangesAsync();

            _context.MealReservations.Add(new MealReservation 
            { 
                UserId = user.Id,
                QRCode = "QR1", 
                Status = MealReservationStatus.Reserved,
                MenuId = menu.Id,
                CafeteriaId = cafe.Id,
                MealType = MealType.Lunch,
                Date = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByQRCodeAsync("QR1");

            // Assert
            result.Should().NotBeNull();
            result!.QRCode.Should().Be("QR1");
        }

        [Fact]
        public async Task GetByQRCodeAsync_ShouldReturnNull_WhenQRCodeNotFound()
        {
            // Act
            var result = await _dal.GetByQRCodeAsync("NONEXISTENT");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ExistsForUserDateMealTypeAsync_ShouldReturnTrue_WhenExists()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
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
                QRCode = "QR1"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.ExistsForUserDateMealTypeAsync(user.Id, DateTime.UtcNow, MealType.Lunch);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsForUserDateMealTypeAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.ExistsForUserDateMealTypeAsync(user.Id, DateTime.UtcNow, MealType.Lunch);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByUserAsync_ShouldReturnUserReservations()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
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
                QRCode = "QR1"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByUserAsync(user.Id);

            // Assert
            result.Should().HaveCount(1);
            result.First().UserId.Should().Be(user.Id);
        }

        [Fact]
        public async Task GetByUserAsync_ShouldFilterByDateRange()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);

            var menu1 = new MealMenu { CafeteriaId = cafe.Id, Date = yesterday, MealType = MealType.Lunch, IsActive = true };
            var menu2 = new MealMenu { CafeteriaId = cafe.Id, Date = today, MealType = MealType.Lunch, IsActive = true };
            var menu3 = new MealMenu { CafeteriaId = cafe.Id, Date = tomorrow, MealType = MealType.Lunch, IsActive = true };
            _context.MealMenus.AddRange(menu1, menu2, menu3);
            await _context.SaveChangesAsync();

            _context.MealReservations.AddRange(
                new MealReservation { UserId = user.Id, MenuId = menu1.Id, CafeteriaId = cafe.Id, MealType = MealType.Lunch, Date = yesterday, Status = MealReservationStatus.Reserved, QRCode = "QR1" },
                new MealReservation { UserId = user.Id, MenuId = menu2.Id, CafeteriaId = cafe.Id, MealType = MealType.Lunch, Date = today, Status = MealReservationStatus.Reserved, QRCode = "QR2" },
                new MealReservation { UserId = user.Id, MenuId = menu3.Id, CafeteriaId = cafe.Id, MealType = MealType.Lunch, Date = tomorrow, Status = MealReservationStatus.Reserved, QRCode = "QR3" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByUserAsync(user.Id, today, today);

            // Assert
            result.Should().HaveCount(1);
            result.First().Date.Date.Should().Be(today);
        }

        [Fact]
        public async Task GetByDateAsync_ShouldReturnReservationsForDate()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            var date = DateTime.UtcNow.Date;
            var menu = new MealMenu { CafeteriaId = cafe.Id, Date = date, MealType = MealType.Lunch, IsActive = true };
            _context.MealMenus.Add(menu);
            await _context.SaveChangesAsync();

            _context.MealReservations.Add(new MealReservation
            {
                UserId = user.Id,
                MenuId = menu.Id,
                CafeteriaId = cafe.Id,
                MealType = MealType.Lunch,
                Date = date,
                Status = MealReservationStatus.Reserved,
                QRCode = "QR1"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByDateAsync(date);

            // Assert
            result.Should().HaveCount(1);
            result.First().Date.Date.Should().Be(date);
        }

        [Fact]
        public async Task GetByDateAsync_ShouldFilterByCafeteria()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var cafe1 = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            var cafe2 = new Cafeteria { Name = "Second", Location = "Building B", Capacity = 50 };
            _context.Cafeterias.AddRange(cafe1, cafe2);
            await _context.SaveChangesAsync();

            var date = DateTime.UtcNow.Date;
            var menu1 = new MealMenu { CafeteriaId = cafe1.Id, Date = date, MealType = MealType.Lunch, IsActive = true };
            var menu2 = new MealMenu { CafeteriaId = cafe2.Id, Date = date, MealType = MealType.Lunch, IsActive = true };
            _context.MealMenus.AddRange(menu1, menu2);
            await _context.SaveChangesAsync();

            _context.MealReservations.AddRange(
                new MealReservation { UserId = user.Id, MenuId = menu1.Id, CafeteriaId = cafe1.Id, MealType = MealType.Lunch, Date = date, Status = MealReservationStatus.Reserved, QRCode = "QR1" },
                new MealReservation { UserId = user.Id, MenuId = menu2.Id, CafeteriaId = cafe2.Id, MealType = MealType.Lunch, Date = date, Status = MealReservationStatus.Reserved, QRCode = "QR2" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByDateAsync(date, cafe1.Id);

            // Assert
            result.Should().HaveCount(1);
            result.First().CafeteriaId.Should().Be(cafe1.Id);
        }

        [Fact]
        public async Task GetDailyReservationCountAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            var date = DateTime.UtcNow.Date;
            var menu1 = new MealMenu { CafeteriaId = cafe.Id, Date = date, MealType = MealType.Lunch, IsActive = true };
            var menu2 = new MealMenu { CafeteriaId = cafe.Id, Date = date, MealType = MealType.Dinner, IsActive = true };
            _context.MealMenus.AddRange(menu1, menu2);
            await _context.SaveChangesAsync();

            _context.MealReservations.AddRange(
                new MealReservation { UserId = user.Id, MenuId = menu1.Id, CafeteriaId = cafe.Id, MealType = MealType.Lunch, Date = date, Status = MealReservationStatus.Reserved, QRCode = "QR1" },
                new MealReservation { UserId = user.Id, MenuId = menu2.Id, CafeteriaId = cafe.Id, MealType = MealType.Dinner, Date = date, Status = MealReservationStatus.Reserved, QRCode = "QR2" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetDailyReservationCountAsync(user.Id, date);

            // Assert
            result.Should().Be(2);
        }

        [Fact]
        public async Task GetDailyReservationCountAsync_ShouldOnlyCountReservedStatus()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            var date = DateTime.UtcNow.Date;
            var menu = new MealMenu { CafeteriaId = cafe.Id, Date = date, MealType = MealType.Lunch, IsActive = true };
            _context.MealMenus.Add(menu);
            await _context.SaveChangesAsync();

            _context.MealReservations.AddRange(
                new MealReservation { UserId = user.Id, MenuId = menu.Id, CafeteriaId = cafe.Id, MealType = MealType.Lunch, Date = date, Status = MealReservationStatus.Reserved, QRCode = "QR1" },
                new MealReservation { UserId = user.Id, MenuId = menu.Id, CafeteriaId = cafe.Id, MealType = MealType.Dinner, Date = date, Status = MealReservationStatus.Used, QRCode = "QR2" },
                new MealReservation { UserId = user.Id, MenuId = menu.Id, CafeteriaId = cafe.Id, MealType = MealType.Breakfast, Date = date, Status = MealReservationStatus.Cancelled, QRCode = "QR3" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetDailyReservationCountAsync(user.Id, date);

            // Assert
            result.Should().Be(1); // Only Reserved status
        }

        [Fact]
        public async Task GetExpiredReservationsAsync_ShouldReturnExpiredReservations()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var cafe = new Cafeteria { Name = "Main", Location = "Building A", Capacity = 100 };
            _context.Cafeterias.Add(cafe);
            await _context.SaveChangesAsync();

            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var today = DateTime.UtcNow.Date;
            var menu1 = new MealMenu { CafeteriaId = cafe.Id, Date = yesterday, MealType = MealType.Lunch, IsActive = true };
            var menu2 = new MealMenu { CafeteriaId = cafe.Id, Date = today, MealType = MealType.Lunch, IsActive = true };
            _context.MealMenus.AddRange(menu1, menu2);
            await _context.SaveChangesAsync();

            _context.MealReservations.AddRange(
                new MealReservation { UserId = user.Id, MenuId = menu1.Id, CafeteriaId = cafe.Id, MealType = MealType.Lunch, Date = yesterday, Status = MealReservationStatus.Reserved, QRCode = "QR1" },
                new MealReservation { UserId = user.Id, MenuId = menu2.Id, CafeteriaId = cafe.Id, MealType = MealType.Lunch, Date = today, Status = MealReservationStatus.Reserved, QRCode = "QR2" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetExpiredReservationsAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Date.Date.Should().Be(yesterday);
        }
    }
}
