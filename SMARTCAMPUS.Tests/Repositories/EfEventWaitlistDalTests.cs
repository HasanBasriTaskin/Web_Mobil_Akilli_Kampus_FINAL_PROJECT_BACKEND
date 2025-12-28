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
    public class EfEventWaitlistDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfEventWaitlistDal _dal;

        public EfEventWaitlistDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfEventWaitlistDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetByEventAndUserAsync_ShouldReturnWaitlist_WhenExists()
        {
            var user = new User { Id = "u1", FullName = "User", IsActive = true };
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var evt = new Event { Id = 1, Title = "Event", Description = "Desc", Location = "Hall", CategoryId = 1, Category = category, CreatedByUserId = "u1", CreatedBy = user, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(1).AddHours(2), IsActive = true };
            var waitlist = new EventWaitlist { EventId = 1, Event = evt, UserId = "u1", User = user, QueuePosition = 1, IsActive = true };

            await _context.Users.AddAsync(user);
            await _context.EventCategories.AddAsync(category);
            await _context.Events.AddAsync(evt);
            await _context.EventWaitlists.AddAsync(waitlist);
            await _context.SaveChangesAsync();

            var result = await _dal.GetByEventAndUserAsync(1, "u1");

            result.Should().NotBeNull();
            result!.EventId.Should().Be(1);
            result.UserId.Should().Be("u1");
        }

        [Fact]
        public async Task GetByEventAndUserAsync_ShouldReturnNull_WhenNotFound()
        {
            var result = await _dal.GetByEventAndUserAsync(999, "u1");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetNextInQueueAsync_ShouldReturnFirstInQueue()
        {
            var user = new User { Id = "u1", FullName = "User", IsActive = true };
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var evt = new Event { Id = 1, Title = "Event", Description = "Desc", Location = "Hall", CategoryId = 1, Category = category, CreatedByUserId = "u1", CreatedBy = user, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(1).AddHours(2), IsActive = true };
            var w1 = new EventWaitlist { EventId = 1, Event = evt, UserId = "u1", User = user, QueuePosition = 1, IsNotified = false, IsActive = true };
            var w2 = new EventWaitlist { EventId = 1, Event = evt, UserId = "u2", User = new User { Id = "u2", FullName = "User2", IsActive = true }, QueuePosition = 2, IsNotified = false, IsActive = true };

            await _context.Users.AddRangeAsync(user, w2.User);
            await _context.EventCategories.AddAsync(category);
            await _context.Events.AddAsync(evt);
            await _context.EventWaitlists.AddRangeAsync(w1, w2);
            await _context.SaveChangesAsync();

            var result = await _dal.GetNextInQueueAsync(1);

            result.Should().NotBeNull();
            result!.QueuePosition.Should().Be(1);
        }

        [Fact]
        public async Task GetNextInQueueAsync_ShouldReturnNull_WhenAllNotified()
        {
            var user = new User { Id = "u1", FullName = "User", IsActive = true };
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var evt = new Event { Id = 1, Title = "Event", Description = "Desc", Location = "Hall", CategoryId = 1, Category = category, CreatedByUserId = "u1", CreatedBy = user, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(1).AddHours(2), IsActive = true };
            var w1 = new EventWaitlist { EventId = 1, Event = evt, UserId = "u1", User = user, QueuePosition = 1, IsNotified = true, IsActive = true };

            await _context.Users.AddAsync(user);
            await _context.EventCategories.AddAsync(category);
            await _context.Events.AddAsync(evt);
            await _context.EventWaitlists.AddAsync(w1);
            await _context.SaveChangesAsync();

            var result = await _dal.GetNextInQueueAsync(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByEventIdAsync_ShouldReturnOrderedList()
        {
            var user1 = new User { Id = "u1", FullName = "User1", IsActive = true };
            var user2 = new User { Id = "u2", FullName = "User2", IsActive = true };
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var evt = new Event { Id = 1, Title = "Event", Description = "Desc", Location = "Hall", CategoryId = 1, Category = category, CreatedByUserId = "u1", CreatedBy = user1, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(1).AddHours(2), IsActive = true };
            var w1 = new EventWaitlist { EventId = 1, Event = evt, UserId = "u1", User = user1, QueuePosition = 2, IsActive = true };
            var w2 = new EventWaitlist { EventId = 1, Event = evt, UserId = "u2", User = user2, QueuePosition = 1, IsActive = true };

            await _context.Users.AddRangeAsync(user1, user2);
            await _context.EventCategories.AddAsync(category);
            await _context.Events.AddAsync(evt);
            await _context.EventWaitlists.AddRangeAsync(w1, w2);
            await _context.SaveChangesAsync();

            var result = await _dal.GetByEventIdAsync(1);

            result.Should().HaveCount(2);
            result.First().QueuePosition.Should().Be(1);
            result.Last().QueuePosition.Should().Be(2);
        }

        [Fact]
        public async Task IsUserInWaitlistAsync_ShouldReturnTrue_WhenExists()
        {
            var user = new User { Id = "u1", FullName = "User", IsActive = true };
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var evt = new Event { Id = 1, Title = "Event", Description = "Desc", Location = "Hall", CategoryId = 1, Category = category, CreatedByUserId = "u1", CreatedBy = user, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(1).AddHours(2), IsActive = true };
            var waitlist = new EventWaitlist { EventId = 1, Event = evt, UserId = "u1", User = user, QueuePosition = 1, IsActive = true };

            await _context.Users.AddAsync(user);
            await _context.EventCategories.AddAsync(category);
            await _context.Events.AddAsync(evt);
            await _context.EventWaitlists.AddAsync(waitlist);
            await _context.SaveChangesAsync();

            var result = await _dal.IsUserInWaitlistAsync(1, "u1");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsUserInWaitlistAsync_ShouldReturnFalse_WhenNotExists()
        {
            var result = await _dal.IsUserInWaitlistAsync(999, "u1");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetMaxPositionAsync_ShouldReturnMaxPosition()
        {
            var user1 = new User { Id = "u1", FullName = "User1", IsActive = true };
            var user2 = new User { Id = "u2", FullName = "User2", IsActive = true };
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var evt = new Event { Id = 1, Title = "Event", Description = "Desc", Location = "Hall", CategoryId = 1, Category = category, CreatedByUserId = "u1", CreatedBy = user1, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(1).AddHours(2), IsActive = true };
            var w1 = new EventWaitlist { EventId = 1, Event = evt, UserId = "u1", User = user1, QueuePosition = 1, IsActive = true };
            var w2 = new EventWaitlist { EventId = 1, Event = evt, UserId = "u2", User = user2, QueuePosition = 5, IsActive = true };

            await _context.Users.AddRangeAsync(user1, user2);
            await _context.EventCategories.AddAsync(category);
            await _context.Events.AddAsync(evt);
            await _context.EventWaitlists.AddRangeAsync(w1, w2);
            await _context.SaveChangesAsync();

            var result = await _dal.GetMaxPositionAsync(1);

            result.Should().Be(5);
        }

        [Fact]
        public async Task GetMaxPositionAsync_ShouldReturnZero_WhenNoWaitlist()
        {
            var result = await _dal.GetMaxPositionAsync(999);

            result.Should().Be(0);
        }
    }
}
