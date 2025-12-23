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
        public async Task GetNextInQueueAsync_ShouldReturnFirstInQueue()
        {
            // Arrange
            var w1 = new EventWaitlist { EventId = 1, QueuePosition = 1, IsNotified = false, IsActive = true };
            _context.EventWaitlists.Add(w1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetNextInQueueAsync(1);

            // Assert
            result.Should().NotBeNull();
        }
    }
}
