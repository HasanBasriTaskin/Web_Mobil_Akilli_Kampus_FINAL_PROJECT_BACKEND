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
    public class EfEventDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfEventDal _dal;

        public EfEventDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfEventDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_ShouldIncludeRelations()
        {
            // Arrange
            var cat = new EventCategory { Name = "Music" };
            var user = new User { Id = "u1", FullName = "Organizer" };
            var evt = new Event { Title = "Concert", Description = "Desc", Location = "Hall", Category = cat, CreatedBy = user, CreatedByUserId = "u1", StartDate = DateTime.Now, EndDate = DateTime.Now.AddHours(2) };

            _context.EventCategories.Add(cat);
            _context.Users.Add(user);
            _context.Events.Add(evt);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByIdWithDetailsAsync(evt.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Category.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_ShouldReturnNull_WhenNotFound()
        {
            // Act
            var result = await _dal.GetByIdWithDetailsAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetEventsFilteredAsync_ShouldFilterByCategory()
        {
            // Arrange
            var cat1 = new EventCategory { Name = "Conference", IsActive = true };
            var cat2 = new EventCategory { Name = "Workshop", IsActive = true };
            _context.EventCategories.AddRange(cat1, cat2);
            await _context.SaveChangesAsync();

            var evt1 = new Event { Title = "Event 1", Description = "Description 1", StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(1).AddHours(2), Location = "Hall", CategoryId = cat1.Id, CreatedByUserId = "admin1", IsActive = true };
            var evt2 = new Event { Title = "Event 2", Description = "Description 2", StartDate = DateTime.UtcNow.AddDays(2), EndDate = DateTime.UtcNow.AddDays(2).AddHours(2), Location = "Hall", CategoryId = cat2.Id, CreatedByUserId = "admin1", IsActive = true };
            _context.Events.AddRange(evt1, evt2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetEventsFilteredAsync(cat1.Id, null, null, null, null, 1, 10);

            // Assert
            result.Should().HaveCount(1);
            result.First().CategoryId.Should().Be(cat1.Id);
        }

        [Fact]
        public async Task GetEventsFilteredAsync_ShouldFilterByDateRange()
        {
            // Arrange
            var cat = new EventCategory { Name = "Conference", IsActive = true };
            _context.EventCategories.Add(cat);
            await _context.SaveChangesAsync();

            var fromDate = DateTime.UtcNow.AddDays(1);
            var toDate = DateTime.UtcNow.AddDays(3);
            var evt1 = new Event { Title = "Event 1", Description = "Description 1", StartDate = fromDate, EndDate = fromDate.AddHours(2), Location = "Hall", CategoryId = cat.Id, CreatedByUserId = "admin1", IsActive = true };
            var evt2 = new Event { Title = "Event 2", Description = "Description 2", StartDate = DateTime.UtcNow.AddDays(5), EndDate = DateTime.UtcNow.AddDays(5).AddHours(2), Location = "Hall", CategoryId = cat.Id, CreatedByUserId = "admin1", IsActive = true };
            _context.Events.AddRange(evt1, evt2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetEventsFilteredAsync(null, fromDate, toDate, null, null, 1, 10);

            // Assert
            result.Should().HaveCount(1);
            result.First().StartDate.Should().BeOnOrAfter(fromDate);
        }

        [Fact]
        public async Task GetEventsFilteredAsync_ShouldFilterByIsFree()
        {
            // Arrange
            var cat = new EventCategory { Name = "Conference", IsActive = true };
            _context.EventCategories.Add(cat);
            await _context.SaveChangesAsync();

            var evt1 = new Event { Title = "Free Event", Description = "Free event description", StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(1).AddHours(2), Location = "Hall", CategoryId = cat.Id, CreatedByUserId = "admin1", Price = 0, IsActive = true };
            var evt2 = new Event { Title = "Paid Event", Description = "Paid event description", StartDate = DateTime.UtcNow.AddDays(2), EndDate = DateTime.UtcNow.AddDays(2).AddHours(2), Location = "Hall", CategoryId = cat.Id, CreatedByUserId = "admin1", Price = 100, IsActive = true };
            _context.Events.AddRange(evt1, evt2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetEventsFilteredAsync(null, null, null, true, null, 1, 10);

            // Assert
            result.Should().HaveCount(1);
            result.First().Price.Should().Be(0);
        }

        [Fact]
        public async Task GetEventsFilteredAsync_ShouldFilterBySearchQuery()
        {
            // Arrange
            var cat = new EventCategory { Name = "Conference", IsActive = true };
            _context.EventCategories.Add(cat);
            await _context.SaveChangesAsync();

            var evt1 = new Event { Title = "Tech Conference", Description = "Technology", StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(1).AddHours(2), Location = "Hall", CategoryId = cat.Id, CreatedByUserId = "admin1", IsActive = true };
            var evt2 = new Event { Title = "Music Event", Description = "Music", StartDate = DateTime.UtcNow.AddDays(2), EndDate = DateTime.UtcNow.AddDays(2).AddHours(2), Location = "Hall", CategoryId = cat.Id, CreatedByUserId = "admin1", IsActive = true };
            _context.Events.AddRange(evt1, evt2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetEventsFilteredAsync(null, null, null, null, "Tech", 1, 10);

            // Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Contain("Tech");
        }

        [Fact]
        public async Task GetEventsFilteredAsync_ShouldPaginate()
        {
            // Arrange
            var cat = new EventCategory { Name = "Conference", IsActive = true };
            _context.EventCategories.Add(cat);
            await _context.SaveChangesAsync();

            for (int i = 0; i < 5; i++)
            {
                _context.Events.Add(new Event 
                { 
                    Title = $"Event {i}", 
                    Description = $"Description {i}",
                    StartDate = DateTime.UtcNow.AddDays(i + 1), 
                    EndDate = DateTime.UtcNow.AddDays(i + 1).AddHours(2), 
                    Location = "Hall", 
                    CategoryId = cat.Id, 
                    CreatedByUserId = "admin1", 
                    IsActive = true 
                });
            }
            await _context.SaveChangesAsync();

            // Act
            var page1 = await _dal.GetEventsFilteredAsync(null, null, null, null, null, 1, 2);
            var page2 = await _dal.GetEventsFilteredAsync(null, null, null, null, null, 2, 2);

            // Assert
            page1.Should().HaveCount(2);
            page2.Should().HaveCount(2);
            page1.Should().NotIntersectWith(page2);
        }

        [Fact]
        public async Task GetEventsCountAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var cat = new EventCategory { Name = "Conference", IsActive = true };
            _context.EventCategories.Add(cat);
            await _context.SaveChangesAsync();

            _context.Events.AddRange(
                new Event { Title = "Event 1", Description = "Description 1", StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(1).AddHours(2), Location = "Hall", CategoryId = cat.Id, CreatedByUserId = "admin1", IsActive = true },
                new Event { Title = "Event 2", Description = "Description 2", StartDate = DateTime.UtcNow.AddDays(2), EndDate = DateTime.UtcNow.AddDays(2).AddHours(2), Location = "Hall", CategoryId = cat.Id, CreatedByUserId = "admin1", IsActive = true },
                new Event { Title = "Event 3", Description = "Description 3", StartDate = DateTime.UtcNow.AddDays(3), EndDate = DateTime.UtcNow.AddDays(3).AddHours(2), Location = "Hall", CategoryId = cat.Id, CreatedByUserId = "admin1", IsActive = false }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetEventsCountAsync(null, null, null, null, null);

            // Assert
            result.Should().Be(2); // Only active events
        }

        [Fact]
        public async Task GetEventsCountAsync_ShouldFilterByCategory()
        {
            // Arrange
            var cat1 = new EventCategory { Name = "Conference", IsActive = true };
            var cat2 = new EventCategory { Name = "Workshop", IsActive = true };
            _context.EventCategories.AddRange(cat1, cat2);
            await _context.SaveChangesAsync();

            _context.Events.AddRange(
                new Event { Title = "Event 1", Description = "Description 1", StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(1).AddHours(2), Location = "Hall", CategoryId = cat1.Id, CreatedByUserId = "admin1", IsActive = true },
                new Event { Title = "Event 2", Description = "Description 2", StartDate = DateTime.UtcNow.AddDays(2), EndDate = DateTime.UtcNow.AddDays(2).AddHours(2), Location = "Hall", CategoryId = cat2.Id, CreatedByUserId = "admin1", IsActive = true }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetEventsCountAsync(cat1.Id, null, null, null, null);

            // Assert
            result.Should().Be(1);
        }
    }
}
