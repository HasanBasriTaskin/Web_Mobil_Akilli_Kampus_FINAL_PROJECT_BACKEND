using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class ScheduleServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IScheduleDal> _mockScheduleDal;
        private readonly Mock<ICourseSectionDal> _mockSectionDal;
        private readonly Mock<IClassroomDal> _mockClassroomDal;
        private readonly ScheduleManager _scheduleManager;

        public ScheduleServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockScheduleDal = new Mock<IScheduleDal>();
            _mockSectionDal = new Mock<ICourseSectionDal>();
            _mockClassroomDal = new Mock<IClassroomDal>();

            _mockUnitOfWork.Setup(u => u.Schedules).Returns(_mockScheduleDal.Object);
            _mockUnitOfWork.Setup(u => u.CourseSections).Returns(_mockSectionDal.Object);
            _mockUnitOfWork.Setup(u => u.Classrooms).Returns(_mockClassroomDal.Object);

            _scheduleManager = new ScheduleManager(_mockUnitOfWork.Object);
        }

        #region GetSchedulesBySectionAsync Tests

        [Fact]
        public async Task GetSchedulesBySectionAsync_WithValidSectionId_ShouldReturnSchedules()
        {
            // Arrange
            var sectionId = 1;
            var schedules = new List<Schedule>
            {
                new Schedule { Id = 1, SectionId = sectionId, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11), IsActive = true },
                new Schedule { Id = 2, SectionId = sectionId, DayOfWeek = DayOfWeek.Wednesday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11), IsActive = true }
            };

            _mockScheduleDal.Setup(d => d.GetBySectionIdAsync(sectionId))
                .ReturnsAsync(schedules);

            // Act
            var result = await _scheduleManager.GetSchedulesBySectionAsync(sectionId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Count.Should().Be(2);
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetSchedulesBySectionAsync_WithNoSchedules_ShouldReturnEmptyList()
        {
            // Arrange
            var sectionId = 999;
            _mockScheduleDal.Setup(d => d.GetBySectionIdAsync(sectionId))
                .ReturnsAsync(new List<Schedule>());

            // Act
            var result = await _scheduleManager.GetSchedulesBySectionAsync(sectionId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Count.Should().Be(0);
        }

        #endregion

        #region GetWeeklyScheduleAsync Tests

        [Fact]
        public async Task GetWeeklyScheduleAsync_WithValidSection_ShouldReturnWeeklySchedule()
        {
            // Arrange
            var sectionId = 1;
            var section = new CourseSection
            {
                Id = sectionId,
                SectionNumber = "A",
                Capacity = 30,
                Course = new Course { Id = 1, Code = "CS101", Name = "Introduction to CS" }
            };

            var schedules = new List<Schedule>
            {
                new Schedule { Id = 1, SectionId = sectionId, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11), IsActive = true },
                new Schedule { Id = 2, SectionId = sectionId, DayOfWeek = DayOfWeek.Wednesday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11), IsActive = true }
            };

            _mockSectionDal.Setup(d => d.GetByIdAsync(sectionId))
                .ReturnsAsync(section);
            _mockScheduleDal.Setup(d => d.GetBySectionIdAsync(sectionId))
                .ReturnsAsync(schedules);

            // Act
            var result = await _scheduleManager.GetWeeklyScheduleAsync(sectionId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Count.Should().Be(2); // Monday and Wednesday
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetWeeklyScheduleAsync_WithInvalidSectionId_ShouldReturnError()
        {
            // Arrange
            var sectionId = 999;
            _mockSectionDal.Setup(d => d.GetByIdAsync(sectionId))
                .ReturnsAsync((CourseSection?)null);

            // Act
            var result = await _scheduleManager.GetWeeklyScheduleAsync(sectionId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeFalse();
            result.Data.Should().BeNull();
            result.StatusCode.Should().Be(404);
            result.Errors.Should().Contain(e => e.Contains("bulunamadı"));
        }

        #endregion

        #region CheckConflictsAsync Tests

        [Fact]
        public async Task CheckConflictsAsync_WithNoConflicts_ShouldReturnEmptyList()
        {
            // Arrange
            var dto = new ScheduleCreateDto
            {
                SectionId = 1,
                ClassroomId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11)
            };

            _mockScheduleDal.Setup(d => d.GetByClassroomIdAsync(dto.ClassroomId, dto.DayOfWeek))
                .ReturnsAsync(new List<Schedule>());

            // Act
            var result = await _scheduleManager.CheckConflictsAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Count.Should().Be(0);
        }

        [Fact]
        public async Task CheckConflictsAsync_WithTimeOverlap_ShouldReturnConflict()
        {
            // Arrange
            var dto = new ScheduleCreateDto
            {
                SectionId = 1,
                ClassroomId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11)
            };

            var existingSchedule = new Schedule
            {
                Id = 1,
                SectionId = 2,
                ClassroomId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(10), // Overlaps with 9-11
                EndTime = TimeSpan.FromHours(12),
                IsActive = true,
                Section = new CourseSection
                {
                    Id = 2,
                    Course = new Course { Code = "CS101", Name = "Test Course" }
                }
            };

            _mockScheduleDal.Setup(d => d.GetConflictingScheduleAsync(
                dto.ClassroomId, dto.DayOfWeek, dto.StartTime, dto.EndTime, null))
                .ReturnsAsync(existingSchedule);

            // Act
            var result = await _scheduleManager.CheckConflictsAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Count.Should().BeGreaterThan(0);
        }

        #endregion

        #region CreateScheduleAsync Tests

        [Fact]
        public async Task CreateScheduleAsync_WithNoConflicts_ShouldCreateSchedule()
        {
            // Arrange
            var dto = new ScheduleCreateDto
            {
                SectionId = 1,
                ClassroomId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11)
            };

            var section = new CourseSection
            {
                Id = 1,
                SectionNumber = "A",
                Capacity = 30,
                Course = new Course { Id = 1, Code = "CS101", Name = "Introduction to CS" }
            };

            var classroom = new Classroom
            {
                Id = 1,
                Building = "A",
                RoomNumber = "101",
                Capacity = 50
            };

            var createdSchedule = new Schedule
            {
                Id = 1,
                SectionId = dto.SectionId,
                ClassroomId = dto.ClassroomId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsActive = true,
                Section = section,
                Classroom = classroom
            };

            _mockScheduleDal.Setup(d => d.GetByClassroomIdAsync(dto.ClassroomId, dto.DayOfWeek))
                .ReturnsAsync(new List<Schedule>());
            _mockSectionDal.Setup(d => d.GetByIdAsync(dto.SectionId))
                .ReturnsAsync(section);
            _mockClassroomDal.Setup(d => d.GetByIdAsync(dto.ClassroomId))
                .ReturnsAsync(classroom);
            _mockScheduleDal.Setup(d => d.AddAsync(It.IsAny<Schedule>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);
            _mockScheduleDal.Setup(d => d.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(createdSchedule);

            // Act
            var result = await _scheduleManager.CreateScheduleAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            _mockScheduleDal.Verify(d => d.AddAsync(It.IsAny<Schedule>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateScheduleAsync_WithConflicts_ShouldReturnError()
        {
            // Arrange
            var dto = new ScheduleCreateDto
            {
                SectionId = 1,
                ClassroomId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11)
            };

            var section = new CourseSection
            {
                Id = 1,
                SectionNumber = "A",
                Capacity = 30,
                Course = new Course { Id = 1, Code = "CS101", Name = "Introduction to CS" }
            };

            var classroom = new Classroom
            {
                Id = 1,
                Building = "A",
                RoomNumber = "101",
                Capacity = 50
            };

            var existingSchedule = new Schedule
            {
                Id = 1,
                SectionId = 2,
                ClassroomId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(12),
                IsActive = true,
                Section = new CourseSection
                {
                    Id = 2,
                    Course = new Course { Code = "CS102", Name = "Test Course" }
                }
            };

            _mockSectionDal.Setup(d => d.GetByIdAsync(dto.SectionId))
                .ReturnsAsync(section);
            _mockClassroomDal.Setup(d => d.GetByIdAsync(dto.ClassroomId))
                .ReturnsAsync(classroom);
            _mockScheduleDal.Setup(d => d.GetConflictingScheduleAsync(
                dto.ClassroomId, dto.DayOfWeek, dto.StartTime, dto.EndTime, null))
                .ReturnsAsync(existingSchedule);

            // Act
            var result = await _scheduleManager.CreateScheduleAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeFalse();
            result.Data.Should().BeNull();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain(e => e.Contains("Çakışma"));
        }

        #endregion
    }
}

