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
            var reg = new EventRegistration { EventId = 1, UserId = "u1", IsActive = true };
            _context.EventRegistrations.Add(reg);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByEventAndUserAsync(1, "u1");

            // Assert
            result.Should().NotBeNull();
        }
    }
}
