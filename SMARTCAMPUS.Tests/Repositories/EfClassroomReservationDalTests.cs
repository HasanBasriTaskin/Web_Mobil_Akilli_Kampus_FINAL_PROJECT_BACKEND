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
            var res1 = new ClassroomReservation { Classroom = room, RequestedByUserId = "u1", StartTime = TimeSpan.Zero, EndTime = TimeSpan.FromHours(1), Purpose = "Meeting 1", ReservationDate = DateTime.Today };

            _context.ClassroomReservations.Add(res1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByUserIdAsync("u1");

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_ShouldIncludeRelations()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var room = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(room);
            await _context.SaveChangesAsync();

            var reservation = new ClassroomReservation 
            { 
                Classroom = room, 
                ClassroomId = room.Id,
                RequestedByUserId = user.Id, 
                StartTime = TimeSpan.FromHours(9), 
                EndTime = TimeSpan.FromHours(11), 
                Purpose = "Meeting", 
                ReservationDate = DateTime.Today,
                Status = ReservationStatus.Pending
            };
            _context.ClassroomReservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByIdWithDetailsAsync(reservation.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Classroom.Should().NotBeNull();
            result.RequestedBy.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByDateAsync_ShouldReturnReservationsForDate()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var room = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(room);
            await _context.SaveChangesAsync();

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            _context.ClassroomReservations.AddRange(
                new ClassroomReservation { Classroom = room, ClassroomId = room.Id, RequestedByUserId = user.Id, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11), Purpose = "Meeting 1", ReservationDate = today, Status = ReservationStatus.Pending },
                new ClassroomReservation { Classroom = room, ClassroomId = room.Id, RequestedByUserId = user.Id, StartTime = TimeSpan.FromHours(14), EndTime = TimeSpan.FromHours(16), Purpose = "Meeting 2", ReservationDate = tomorrow, Status = ReservationStatus.Pending }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByDateAsync(today);

            // Assert
            result.Should().HaveCount(1);
            result.First().ReservationDate.Date.Should().Be(today);
        }

        [Fact]
        public async Task GetByDateAsync_ShouldFilterByClassroom()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var room1 = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            var room2 = new Classroom { Building = "B", RoomNumber = "201", Capacity = 30 };
            _context.Classrooms.AddRange(room1, room2);
            await _context.SaveChangesAsync();

            var date = DateTime.Today;
            _context.ClassroomReservations.AddRange(
                new ClassroomReservation { Classroom = room1, ClassroomId = room1.Id, RequestedByUserId = user.Id, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11), Purpose = "Meeting 1", ReservationDate = date, Status = ReservationStatus.Pending },
                new ClassroomReservation { Classroom = room2, ClassroomId = room2.Id, RequestedByUserId = user.Id, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11), Purpose = "Meeting 2", ReservationDate = date, Status = ReservationStatus.Pending }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByDateAsync(date, room1.Id);

            // Assert
            result.Should().HaveCount(1);
            result.First().ClassroomId.Should().Be(room1.Id);
        }

        [Fact]
        public async Task GetPendingAsync_ShouldReturnPendingReservations()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var room = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(room);
            await _context.SaveChangesAsync();

            _context.ClassroomReservations.AddRange(
                new ClassroomReservation { Classroom = room, ClassroomId = room.Id, RequestedByUserId = user.Id, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11), Purpose = "Meeting 1", ReservationDate = DateTime.Today, Status = ReservationStatus.Pending },
                new ClassroomReservation { Classroom = room, ClassroomId = room.Id, RequestedByUserId = user.Id, StartTime = TimeSpan.FromHours(14), EndTime = TimeSpan.FromHours(16), Purpose = "Meeting 2", ReservationDate = DateTime.Today, Status = ReservationStatus.Approved },
                new ClassroomReservation { Classroom = room, ClassroomId = room.Id, RequestedByUserId = user.Id, StartTime = TimeSpan.FromHours(17), EndTime = TimeSpan.FromHours(19), Purpose = "Meeting 3", ReservationDate = DateTime.Today, Status = ReservationStatus.Pending }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetPendingAsync();

            // Assert
            result.Should().HaveCount(2);
            result.All(r => r.Status == ReservationStatus.Pending).Should().BeTrue();
        }

        [Fact]
        public async Task HasConflictAsync_ShouldReturnTrue_WhenConflictExists()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var room = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(room);
            await _context.SaveChangesAsync();

            var date = DateTime.Today;
            _context.ClassroomReservations.Add(new ClassroomReservation 
            { 
                Classroom = room, 
                ClassroomId = room.Id, 
                RequestedByUserId = user.Id, 
                StartTime = TimeSpan.FromHours(9), 
                EndTime = TimeSpan.FromHours(11), 
                Purpose = "Meeting", 
                ReservationDate = date, 
                Status = ReservationStatus.Approved 
            });
            await _context.SaveChangesAsync();

            // Act - Overlapping time
            var result = await _dal.HasConflictAsync(room.Id, date, TimeSpan.FromHours(10), TimeSpan.FromHours(12));

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasConflictAsync_ShouldReturnFalse_WhenNoConflict()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var room = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(room);
            await _context.SaveChangesAsync();

            var date = DateTime.Today;
            _context.ClassroomReservations.Add(new ClassroomReservation 
            { 
                Classroom = room, 
                ClassroomId = room.Id, 
                RequestedByUserId = user.Id, 
                StartTime = TimeSpan.FromHours(9), 
                EndTime = TimeSpan.FromHours(11), 
                Purpose = "Meeting", 
                ReservationDate = date, 
                Status = ReservationStatus.Approved 
            });
            await _context.SaveChangesAsync();

            // Act - Non-overlapping time
            var result = await _dal.HasConflictAsync(room.Id, date, TimeSpan.FromHours(13), TimeSpan.FromHours(15));

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasConflictAsync_ShouldExcludeId_WhenProvided()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var room = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(room);
            await _context.SaveChangesAsync();

            var date = DateTime.Today;
            var existing = new ClassroomReservation 
            { 
                Classroom = room, 
                ClassroomId = room.Id, 
                RequestedByUserId = user.Id, 
                StartTime = TimeSpan.FromHours(9), 
                EndTime = TimeSpan.FromHours(11), 
                Purpose = "Meeting", 
                ReservationDate = date, 
                Status = ReservationStatus.Approved 
            };
            _context.ClassroomReservations.Add(existing);
            await _context.SaveChangesAsync();

            // Act - Same time but exclude existing
            var result = await _dal.HasConflictAsync(room.Id, date, TimeSpan.FromHours(9), TimeSpan.FromHours(11), existing.Id);

            // Assert
            result.Should().BeFalse(); // Should not conflict with itself
        }

        [Fact]
        public async Task GetConflictsAsync_ShouldReturnConflictingReservations()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var room = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(room);
            await _context.SaveChangesAsync();

            var date = DateTime.Today;
            _context.ClassroomReservations.AddRange(
                new ClassroomReservation { Classroom = room, ClassroomId = room.Id, RequestedByUserId = user.Id, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11), Purpose = "Meeting 1", ReservationDate = date, Status = ReservationStatus.Approved },
                new ClassroomReservation { Classroom = room, ClassroomId = room.Id, RequestedByUserId = user.Id, StartTime = TimeSpan.FromHours(13), EndTime = TimeSpan.FromHours(15), Purpose = "Meeting 2", ReservationDate = date, Status = ReservationStatus.Approved }
            );
            await _context.SaveChangesAsync();

            // Act - Overlapping with first
            var result = await _dal.GetConflictsAsync(room.Id, date, TimeSpan.FromHours(10), TimeSpan.FromHours(12));

            // Assert
            result.Should().HaveCount(1);
            result.First().StartTime.Should().Be(TimeSpan.FromHours(9));
        }

        [Fact]
        public async Task HasConflictAsync_ShouldOnlyCheckApprovedReservations()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@test.com", FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var room = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(room);
            await _context.SaveChangesAsync();

            var date = DateTime.Today;
            _context.ClassroomReservations.Add(new ClassroomReservation 
            { 
                Classroom = room, 
                ClassroomId = room.Id, 
                RequestedByUserId = user.Id, 
                StartTime = TimeSpan.FromHours(9), 
                EndTime = TimeSpan.FromHours(11), 
                Purpose = "Meeting", 
                ReservationDate = date, 
                Status = ReservationStatus.Pending 
            });
            await _context.SaveChangesAsync();

            // Act - Overlapping time but status is Pending
            var result = await _dal.HasConflictAsync(room.Id, date, TimeSpan.FromHours(10), TimeSpan.FromHours(12));

            // Assert
            result.Should().BeFalse(); // Pending reservations should not cause conflicts
        }
    }
}
