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
            var evt = new Event { Title = "Concert", Description = "Desc", Location = "Hall", Category = cat, CreatedBy = user, CreatedById = "u1" };

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
    }
}
