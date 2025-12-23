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
    public class EfClassroomReservationDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfClassroomReservationDal _dal;

        public EfClassroomReservationDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfClassroomReservationDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnUserReservations()
        {
            // Arrange
            var room = new Classroom { Building = "A", RoomNumber = "101" };
            var res1 = new ClassroomReservation { Classroom = room, RequestedByUserId = "u1", StartTime = TimeSpan.Zero, EndTime = TimeSpan.FromHours(1), Title = "Meeting 1" };

            _context.ClassroomReservations.Add(res1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByUserIdAsync("u1");

            // Assert
            result.Should().HaveCount(1);
        }
    }
}
