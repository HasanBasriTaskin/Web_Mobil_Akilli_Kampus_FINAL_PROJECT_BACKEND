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
    public class EfMealReservationDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfMealReservationDal _dal;

        public EfMealReservationDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfMealReservationDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetByQRCodeAsync_ShouldReturnReservation()
        {
            // Arrange
            _context.MealReservations.Add(new MealReservation { QRCode = "QR1", Status = MealReservationStatus.Reserved });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByQRCodeAsync("QR1");

            // Assert
            result.Should().NotBeNull();
        }
    }
}
