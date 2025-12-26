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
    public class EfWalletTransactionDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfWalletTransactionDal _dal;

        public EfWalletTransactionDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfWalletTransactionDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetByWalletIdPagedAsync_ShouldReturnPagedTransactions()
        {
            // Arrange
            _context.WalletTransactions.Add(new WalletTransaction { WalletId = 1, Amount = 10, TransactionDate = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByWalletIdPagedAsync(1, 1, 1);

            // Assert
            result.Should().HaveCount(1);
        }
    }
}
