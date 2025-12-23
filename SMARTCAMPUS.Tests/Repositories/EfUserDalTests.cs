using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs.User;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Repositories
{
    public class EfUserDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfUserDal _dal;

        public EfUserDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfUserDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetUsersWithRolesAsync_ShouldFilterBySearch()
        {
            // Arrange
            _context.Users.Add(new User { Id = "u1", FullName = "John Doe", Email = "john@test.com" });
            await _context.SaveChangesAsync();

            var query = new UserQueryParameters { Search = "John" };

            // Act
            var result = await _dal.GetUsersWithRolesAsync(query);

            // Assert
            result.Users.Should().HaveCount(1);
        }
    }
}
