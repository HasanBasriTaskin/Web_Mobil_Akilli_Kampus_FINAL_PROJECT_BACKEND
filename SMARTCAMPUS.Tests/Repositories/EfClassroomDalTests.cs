using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Repositories
{
    public class EfClassroomDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfClassroomDal _repository;

        public EfClassroomDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _repository = new EfClassroomDal(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetByBuildingAndRoomAsync_ShouldReturnClassroom_WhenExists()
        {
            // Arrange
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 30 };
            await _context.Classrooms.AddAsync(classroom);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByBuildingAndRoomAsync("A", "101");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetByBuildingAndRoomAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetByBuildingAndRoomAsync("B", "202");

            // Assert
            result.Should().BeNull();
        }
    }
}
