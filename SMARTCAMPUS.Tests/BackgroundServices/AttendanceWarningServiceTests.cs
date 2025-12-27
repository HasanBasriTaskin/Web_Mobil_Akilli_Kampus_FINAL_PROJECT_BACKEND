using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SMARTCAMPUS.API.BackgroundServices;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using System.Reflection;
using Xunit;

namespace SMARTCAMPUS.Tests.BackgroundServices
{
    public class AttendanceWarningServiceTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly Mock<IAdvancedNotificationService> _mockNotificationService;
        private readonly Mock<ILogger<AttendanceWarningService>> _mockLogger;
        private readonly IServiceProvider _serviceProvider;
        private readonly AttendanceWarningService _service;

        public AttendanceWarningServiceTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _mockNotificationService = new Mock<IAdvancedNotificationService>();
            _mockLogger = new Mock<ILogger<AttendanceWarningService>>();

            var services = new ServiceCollection();
            services.AddSingleton(_context);
            services.AddSingleton(_mockNotificationService.Object);
            _serviceProvider = services.BuildServiceProvider();

            _service = new AttendanceWarningService(_serviceProvider, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void Constructor_ShouldInitialize()
        {
            _service.Should().NotBeNull();
        }

        [Fact]
        public async Task CheckAttendanceAndNotifyAsync_ShouldSendWarning_WhenAbsenteeRateAboveThreshold()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "Student", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var student = new Student { Id = 1, UserId = "u1", User = user, StudentNumber = "S1", DepartmentId = 1, Department = dept, IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Enrolled, IsActive = true };
            
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            section.InstructorId = 1;
            section.Instructor = instructor;

            // 10 oturum var, öğrenci sadece 2'sine katılmış (%80 devamsızlık)
            var sessions = Enumerable.Range(1, 10).Select(i => new AttendanceSession
            {
                Id = i,
                SectionId = 1,
                Section = section,
                InstructorId = 1,
                Instructor = instructor,
                Date = DateTime.UtcNow.AddDays(-i).Date,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(10, 30, 0),
                Latitude = 0,
                Longitude = 0,
                GeofenceRadius = 15,
                Status = AttendanceSessionStatus.Closed,
                IsActive = true
            }).ToList();

            var records = Enumerable.Range(1, 2).Select(i => new AttendanceRecord
            {
                Id = i,
                StudentId = 1,
                Student = student,
                SessionId = i,
                Session = sessions[i - 1],
                CheckInTime = DateTime.UtcNow,
                Latitude = 0,
                Longitude = 0,
                DistanceFromCenter = 0,
                IsActive = true,
                IsFlagged = false
            }).ToList();

            _context.Users.Add(user);
            _context.Departments.Add(dept);
            _context.Faculties.Add(instructor);
            _context.Students.Add(student);
            _context.Courses.Add(course);
            _context.CourseSections.Add(section);
            _context.Enrollments.Add(enrollment);
            _context.AttendanceSessions.AddRange(sessions);
            _context.AttendanceRecords.AddRange(records);
            await _context.SaveChangesAsync();

            _mockNotificationService.Setup(x => x.SendNotificationAsync(It.IsAny<SMARTCAMPUS.EntityLayer.DTOs.Notifications.CreateNotificationDto>()))
                .ReturnsAsync(new SMARTCAMPUS.BusinessLayer.Common.Response<SMARTCAMPUS.EntityLayer.DTOs.Notifications.NotificationDto> { IsSuccessful = true });

            // Act
            var method = typeof(AttendanceWarningService).GetMethod("CheckAttendanceAndNotifyAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method!.Invoke(_service, null)!;

            // Assert
            _mockNotificationService.Verify(x => x.SendNotificationAsync(It.Is<SMARTCAMPUS.EntityLayer.DTOs.Notifications.CreateNotificationDto>(
                dto => dto.Title == "Devamsızlık Uyarısı" && dto.UserId == "u1")), Times.Once);
        }

        [Fact]
        public async Task CheckAttendanceAndNotifyAsync_ShouldNotSendWarning_WhenAbsenteeRateBelowThreshold()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "Student", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var student = new Student { Id = 1, UserId = "u1", User = user, StudentNumber = "S1", DepartmentId = 1, Department = dept, IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Enrolled, IsActive = true };
            
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            section.InstructorId = 1;
            section.Instructor = instructor;

            // 10 oturum var, öğrenci 9'una katılmış (%10 devamsızlık - eşik altı)
            var sessions = Enumerable.Range(1, 10).Select(i => new AttendanceSession
            {
                Id = i,
                SectionId = 1,
                Section = section,
                InstructorId = 1,
                Instructor = instructor,
                Date = DateTime.UtcNow.AddDays(-i).Date,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(10, 30, 0),
                Latitude = 0,
                Longitude = 0,
                GeofenceRadius = 15,
                Status = AttendanceSessionStatus.Closed,
                IsActive = true
            }).ToList();

            var records = Enumerable.Range(1, 9).Select(i => new AttendanceRecord
            {
                Id = i,
                StudentId = 1,
                Student = student,
                SessionId = i,
                Session = sessions[i - 1],
                CheckInTime = DateTime.UtcNow,
                Latitude = 0,
                Longitude = 0,
                DistanceFromCenter = 0,
                IsActive = true,
                IsFlagged = false
            }).ToList();

            _context.Users.Add(user);
            _context.Departments.Add(dept);
            _context.Faculties.Add(instructor);
            _context.Students.Add(student);
            _context.Courses.Add(course);
            _context.CourseSections.Add(section);
            _context.Enrollments.Add(enrollment);
            _context.AttendanceSessions.AddRange(sessions);
            _context.AttendanceRecords.AddRange(records);
            await _context.SaveChangesAsync();

            // Act
            var method = typeof(AttendanceWarningService).GetMethod("CheckAttendanceAndNotifyAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method!.Invoke(_service, null)!;

            // Assert
            _mockNotificationService.Verify(x => x.SendNotificationAsync(It.IsAny<SMARTCAMPUS.EntityLayer.DTOs.Notifications.CreateNotificationDto>()), Times.Never);
        }
    }
}

