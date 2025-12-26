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
    public class EfScheduleDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfScheduleDal _dal;

        public EfScheduleDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfScheduleDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetBySectionIdAsync_ShouldReturnActiveSchedules()
        {
            // Arrange
            _context.Schedules.Add(new Schedule { SectionId = 1, IsActive = true, DayOfWeek = DayOfWeek.Monday });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetBySectionIdAsync(1);

            // Assert
            result.Should().HaveCount(1);
        }
    }
}
