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
    public class EfEventRegistrationDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfEventRegistrationDal _dal;

        public EfEventRegistrationDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfEventRegistrationDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetByEventAndUserAsync_ShouldReturnRegistration()
        {
            // Arrange
            var category = new EventCategory { Name = "Conference", IsActive = true };
            _context.EventCategories.Add(category);
            await _context.SaveChangesAsync();

            var evt = new Event 
            { 
                Title = "Test Event", 
                Description = "Test", 
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Test Location",
                Capacity = 100,
                CategoryId = category.Id,
                CreatedByUserId = "admin1",
                IsActive = true
            };
            _context.Events.Add(evt);
            await _context.SaveChangesAsync();

            var reg = new EventRegistration 
            { 
                EventId = evt.Id, 
                UserId = "u1", 
                IsActive = true,
                QRCode = "EVENT-1-ABC123",
                RegistrationDate = DateTime.UtcNow
            };
            _context.EventRegistrations.Add(reg);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByEventAndUserAsync(evt.Id, "u1");

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByEventAndUserAsync_ShouldReturnNull_WhenNotFound()
        {
            // Act
            var result = await _dal.GetByEventAndUserAsync(999, "nonexistent");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByQRCodeAsync_ShouldReturnRegistration()
        {
            // Arrange
            var category = new EventCategory { Name = "Conference", IsActive = true };
            _context.EventCategories.Add(category);
            await _context.SaveChangesAsync();

            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var evt = new Event 
            { 
                Title = "Test Event", 
                Description = "Test", 
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Test Location",
                Capacity = 100,
                CategoryId = category.Id,
                CreatedByUserId = "admin1",
                IsActive = true
            };
            _context.Events.Add(evt);
            await _context.SaveChangesAsync();

            var reg = new EventRegistration 
            { 
                EventId = evt.Id, 
                UserId = user.Id, 
                IsActive = true,
                QRCode = "EVENT-1-ABC123",
                RegistrationDate = DateTime.UtcNow
            };
            _context.EventRegistrations.Add(reg);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByQRCodeAsync("EVENT-1-ABC123");

            // Assert
            result.Should().NotBeNull();
            result!.QRCode.Should().Be("EVENT-1-ABC123");
            result.Event.Should().NotBeNull();
            result.User.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByQRCodeAsync_ShouldReturnNull_WhenNotFound()
        {
            // Act
            var result = await _dal.GetByQRCodeAsync("NONEXISTENT");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByEventIdAsync_ShouldReturnRegistrationsForEvent()
        {
            // Arrange
            var category = new EventCategory { Name = "Conference", IsActive = true };
            _context.EventCategories.Add(category);
            await _context.SaveChangesAsync();

            var user1 = new User { Id = "u1", Email = "test1@test.com", FullName = "User 1" };
            var user2 = new User { Id = "u2", Email = "test2@test.com", FullName = "User 2" };
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var evt = new Event 
            { 
                Title = "Test Event", 
                Description = "Test", 
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Test Location",
                Capacity = 100,
                CategoryId = category.Id,
                CreatedByUserId = "admin1",
                IsActive = true
            };
            _context.Events.Add(evt);
            await _context.SaveChangesAsync();

            _context.EventRegistrations.AddRange(
                new EventRegistration { EventId = evt.Id, UserId = user1.Id, IsActive = true, QRCode = "QR1", RegistrationDate = DateTime.UtcNow },
                new EventRegistration { EventId = evt.Id, UserId = user2.Id, IsActive = true, QRCode = "QR2", RegistrationDate = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByEventIdAsync(evt.Id);

            // Assert
            result.Should().HaveCount(2);
            result.All(r => r.EventId == evt.Id).Should().BeTrue();
        }

        [Fact]
        public async Task GetByEventIdAsync_ShouldOnlyReturnActiveRegistrations()
        {
            // Arrange
            var category = new EventCategory { Name = "Conference", IsActive = true };
            _context.EventCategories.Add(category);
            await _context.SaveChangesAsync();

            var user1 = new User { Id = "u1", Email = "test1@test.com", FullName = "User 1" };
            var user2 = new User { Id = "u2", Email = "test2@test.com", FullName = "User 2" };
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var evt = new Event 
            { 
                Title = "Test Event", 
                Description = "Test", 
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Test Location",
                Capacity = 100,
                CategoryId = category.Id,
                CreatedByUserId = "admin1",
                IsActive = true
            };
            _context.Events.Add(evt);
            await _context.SaveChangesAsync();

            _context.EventRegistrations.AddRange(
                new EventRegistration { EventId = evt.Id, UserId = user1.Id, IsActive = true, QRCode = "QR1", RegistrationDate = DateTime.UtcNow },
                new EventRegistration { EventId = evt.Id, UserId = user2.Id, IsActive = false, QRCode = "QR2", RegistrationDate = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByEventIdAsync(evt.Id);

            // Assert
            result.Should().HaveCount(1);
            result.First().IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnUserRegistrations()
        {
            // Arrange
            var category = new EventCategory { Name = "Conference", IsActive = true };
            _context.EventCategories.Add(category);
            await _context.SaveChangesAsync();

            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var evt1 = new Event { Title = "Event 1", Description = "Description 1", StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(1).AddHours(2), Location = "Hall", CategoryId = category.Id, CreatedByUserId = "admin1", IsActive = true };
            var evt2 = new Event { Title = "Event 2", Description = "Description 2", StartDate = DateTime.UtcNow.AddDays(2), EndDate = DateTime.UtcNow.AddDays(2).AddHours(2), Location = "Hall", CategoryId = category.Id, CreatedByUserId = "admin1", IsActive = true };
            _context.Events.AddRange(evt1, evt2);
            await _context.SaveChangesAsync();

            _context.EventRegistrations.AddRange(
                new EventRegistration { EventId = evt1.Id, UserId = user.Id, IsActive = true, QRCode = "QR1", RegistrationDate = DateTime.UtcNow },
                new EventRegistration { EventId = evt2.Id, UserId = user.Id, IsActive = true, QRCode = "QR2", RegistrationDate = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByUserIdAsync(user.Id);

            // Assert
            result.Should().HaveCount(2);
            result.All(r => r.UserId == user.Id).Should().BeTrue();
            result.First().Event.StartDate.Should().BeAfter(result.Last().Event.StartDate); // Should be ordered descending
        }

        [Fact]
        public async Task IsUserRegisteredAsync_ShouldReturnTrue_WhenRegistered()
        {
            // Arrange
            var category = new EventCategory { Name = "Conference", IsActive = true };
            _context.EventCategories.Add(category);
            await _context.SaveChangesAsync();

            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var evt = new Event 
            { 
                Title = "Test Event", 
                Description = "Test", 
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Test Location",
                Capacity = 100,
                CategoryId = category.Id,
                CreatedByUserId = "admin1",
                IsActive = true
            };
            _context.Events.Add(evt);
            await _context.SaveChangesAsync();

            _context.EventRegistrations.Add(new EventRegistration 
            { 
                EventId = evt.Id, 
                UserId = user.Id, 
                IsActive = true, 
                QRCode = "QR1", 
                RegistrationDate = DateTime.UtcNow 
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.IsUserRegisteredAsync(evt.Id, user.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsUserRegisteredAsync_ShouldReturnFalse_WhenNotRegistered()
        {
            // Arrange
            var category = new EventCategory { Name = "Conference", IsActive = true };
            _context.EventCategories.Add(category);
            await _context.SaveChangesAsync();

            var evt = new Event 
            { 
                Title = "Test Event", 
                Description = "Test", 
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Test Location",
                Capacity = 100,
                CategoryId = category.Id,
                CreatedByUserId = "admin1",
                IsActive = true
            };
            _context.Events.Add(evt);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.IsUserRegisteredAsync(evt.Id, "nonexistent");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsUserRegisteredAsync_ShouldReturnFalse_WhenInactive()
        {
            // Arrange
            var category = new EventCategory { Name = "Conference", IsActive = true };
            _context.EventCategories.Add(category);
            await _context.SaveChangesAsync();

            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var evt = new Event 
            { 
                Title = "Test Event", 
                Description = "Test", 
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Test Location",
                Capacity = 100,
                CategoryId = category.Id,
                CreatedByUserId = "admin1",
                IsActive = true
            };
            _context.Events.Add(evt);
            await _context.SaveChangesAsync();

            _context.EventRegistrations.Add(new EventRegistration 
            { 
                EventId = evt.Id, 
                UserId = user.Id, 
                IsActive = false, 
                QRCode = "QR1", 
                RegistrationDate = DateTime.UtcNow 
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.IsUserRegisteredAsync(evt.Id, user.Id);

            // Assert
            result.Should().BeFalse(); // Inactive registration should not count
        }
    }
}
