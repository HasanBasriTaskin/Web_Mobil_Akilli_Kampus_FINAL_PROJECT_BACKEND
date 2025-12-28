using FluentAssertions;
using Moq;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class ClassroomReservationManagerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IClassroomReservationDal> _mockReservationDal;
        private readonly Mock<IClassroomDal> _mockClassroomDal;
        private readonly Mock<IScheduleDal> _mockScheduleDal;
        private readonly ClassroomReservationManager _manager;

        public ClassroomReservationManagerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockReservationDal = new Mock<IClassroomReservationDal>();
            _mockClassroomDal = new Mock<IClassroomDal>();
            _mockScheduleDal = new Mock<IScheduleDal>();
            _mockUnitOfWork.Setup(u => u.ClassroomReservations).Returns(_mockReservationDal.Object);
            _mockUnitOfWork.Setup(u => u.Classrooms).Returns(_mockClassroomDal.Object);
            _mockUnitOfWork.Setup(u => u.Schedules).Returns(_mockScheduleDal.Object);
            _manager = new ClassroomReservationManager(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetMyReservationsAsync_ShouldReturnSuccess()
        {
            var reservations = new List<ClassroomReservation>
            {
                new ClassroomReservation { Id = 1, RequestedByUserId = "user1", Status = ReservationStatus.Pending }
            };
            _mockReservationDal.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync(reservations);

            var result = await _manager.GetMyReservationsAsync("user1");

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task CreateReservationAsync_ShouldReturnSuccess_WhenValid()
        {
            var nextWeekday = DateTime.Today.AddDays(1);
            while (nextWeekday.DayOfWeek == DayOfWeek.Saturday || nextWeekday.DayOfWeek == DayOfWeek.Sunday)
                nextWeekday = nextWeekday.AddDays(1);

            var dto = new ClassroomReservationCreateDto
            {
                ClassroomId = 1,
                ReservationDate = nextWeekday,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(12, 0, 0),
                Purpose = "Meeting"
            };
            var classroom = new Classroom { Id = 1, IsActive = true };
            _mockClassroomDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(classroom);
            _mockScheduleDal.Setup(x => x.GetConflictingScheduleAsync(1, nextWeekday.DayOfWeek, dto.StartTime, dto.EndTime, null))
                .ReturnsAsync((Schedule)null!);
            _mockReservationDal.Setup(x => x.HasConflictAsync(1, nextWeekday, dto.StartTime, dto.EndTime, null))
                .ReturnsAsync(false);
            _mockReservationDal.Setup(x => x.AddAsync(It.IsAny<ClassroomReservation>())).Returns(Task.CompletedTask);
            _mockReservationDal.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new ClassroomReservation { Id = id, Status = ReservationStatus.Pending });
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.CreateReservationAsync("user1", dto);

            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task CreateReservationAsync_ShouldReturnFail_WhenWeekend()
        {
            var saturday = DateTime.Today.AddDays((int)DayOfWeek.Saturday - (int)DateTime.Today.DayOfWeek);
            var dto = new ClassroomReservationCreateDto
            {
                ClassroomId = 1,
                ReservationDate = saturday,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(12, 0, 0)
            };

            var result = await _manager.CreateReservationAsync("user1", dto);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task CancelReservationAsync_ShouldReturnSuccess_WhenValid()
        {
            var reservation = new ClassroomReservation
            {
                Id = 1,
                RequestedByUserId = "user1",
                Status = ReservationStatus.Pending
            };
            _mockReservationDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(reservation);
            _mockReservationDal.Setup(x => x.Update(It.IsAny<ClassroomReservation>()));
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.CancelReservationAsync("user1", 1);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task ApproveReservationAsync_ShouldReturnSuccess_WhenValid()
        {
            var reservation = new ClassroomReservation
            {
                Id = 1,
                Status = ReservationStatus.Pending,
                ClassroomId = 1,
                ReservationDate = DateTime.Today,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(12, 0, 0)
            };
            _mockReservationDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(reservation);
            _mockReservationDal.Setup(x => x.HasConflictAsync(1, reservation.ReservationDate, reservation.StartTime, reservation.EndTime, 1))
                .ReturnsAsync(false);
            _mockReservationDal.Setup(x => x.Update(It.IsAny<ClassroomReservation>()));
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.ApproveReservationAsync("admin1", 1, null);

            result.IsSuccessful.Should().BeTrue();
        }
    }
}

