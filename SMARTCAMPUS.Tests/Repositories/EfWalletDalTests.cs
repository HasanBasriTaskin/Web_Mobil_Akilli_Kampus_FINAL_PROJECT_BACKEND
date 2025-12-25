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
    public class EfWalletDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfWalletDal _dal;

        public EfWalletDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfWalletDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnWallet()
        {
            // Arrange
            _context.Wallets.Add(new Wallet { UserId = "u1", Balance = 100 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByUserIdAsync("u1");

            // Assert
            result.Should().NotBeNull();
        }
    }
}
