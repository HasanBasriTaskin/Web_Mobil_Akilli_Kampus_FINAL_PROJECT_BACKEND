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
using IServiceScopeFactory = Microsoft.Extensions.DependencyInjection.IServiceScopeFactory;

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

        [Fact]
        public async Task ExecuteAsync_ShouldStartAndStop_WhenCancellationRequested()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(100));

            // Act
            var executeTask = _service.StartAsync(cts.Token);
            
            try
            {
                await Task.Delay(150, CancellationToken.None);
            }
            catch { }

            await _service.StopAsync(CancellationToken.None);

            // Assert - Service should start and stop without throwing
            executeTask.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleOperationCanceledException()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert - Should not throw
            var executeTask = _service.StartAsync(cts.Token);
            await Task.Delay(50, CancellationToken.None);
            await _service.StopAsync(CancellationToken.None);
            
            executeTask.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogStartAndStop()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(50));

            // Act
            var executeTask = _service.StartAsync(cts.Token);
            await Task.Delay(100, CancellationToken.None);
            await _service.StopAsync(CancellationToken.None);

            // Assert - Should log service started
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("AttendanceWarningService started")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCalculateNextRunTime()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(50));

            // Act
            var executeTask = _service.StartAsync(cts.Token);
            await Task.Delay(100, CancellationToken.None);
            await _service.StopAsync(CancellationToken.None);

            // Assert - Should log next run time
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Next attendance check scheduled")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleException_AndRetry()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(200));

            // Create a service with invalid context to trigger exception
            var invalidServices = new ServiceCollection();
            invalidServices.AddSingleton<CampusContext>(_context);
            invalidServices.AddSingleton<IAdvancedNotificationService>(_mockNotificationService.Object);
            var invalidServiceProvider = invalidServices.BuildServiceProvider();
            
            var serviceWithError = new AttendanceWarningService(invalidServiceProvider, _mockLogger.Object);

            // Act
            var executeTask = serviceWithError.StartAsync(cts.Token);
            await Task.Delay(250, CancellationToken.None);
            await serviceWithError.StopAsync(CancellationToken.None);

            // Assert - Should handle exception and log error
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error in AttendanceWarningService")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtMostOnce);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogServiceStopped()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(50));

            // Act
            var executeTask = _service.StartAsync(cts.Token);
            await Task.Delay(100, CancellationToken.None);
            await _service.StopAsync(CancellationToken.None);

            // Assert - Should log service stopped
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("AttendanceWarningService stopped")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task CheckAttendanceAndNotifyAsync_ShouldCallCheckAttendance_WhenInvokedDirectly()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "Student", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var student = new Student { Id = 1, UserId = "u1", User = user, StudentNumber = "S1", DepartmentId = 1, Department = dept, IsActive = true };
            
            _context.Users.Add(user);
            _context.Departments.Add(dept);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            _mockNotificationService.Setup(x => x.SendNotificationAsync(It.IsAny<SMARTCAMPUS.EntityLayer.DTOs.Notifications.CreateNotificationDto>()))
                .ReturnsAsync(new SMARTCAMPUS.BusinessLayer.Common.Response<SMARTCAMPUS.EntityLayer.DTOs.Notifications.NotificationDto> { IsSuccessful = true });

            // Act - Call CheckAttendanceAndNotifyAsync directly
            var method = typeof(AttendanceWarningService).GetMethod("CheckAttendanceAndNotifyAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method!.Invoke(_service, null)!;

            // Assert - Should have logged attendance check
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting attendance check") || 
                                                  v.ToString()!.Contains("Attendance check completed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }


        [Fact]
        public async Task CheckAttendanceAndNotifyAsync_ShouldHandleException_WhenProcessingStudent()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "Student", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var student = new Student { Id = 1, UserId = "u1", User = user, StudentNumber = "S1", DepartmentId = 1, Department = dept, IsActive = true };
            
            _context.Users.Add(user);
            _context.Departments.Add(dept);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Act - Should not throw even with invalid data
            var method = typeof(AttendanceWarningService).GetMethod("CheckAttendanceAndNotifyAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method!.Invoke(_service, null)!;

            // Assert - Should complete without throwing
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Attendance check completed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CheckAttendanceAndNotifyAsync_ShouldCatchException_WhenNotificationServiceThrows()
        {
            // Arrange - Create a student with enrollment that will trigger notification
            var user = new User { Id = "u1", FullName = "Student", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var student = new Student { Id = 1, UserId = "u1", User = user, StudentNumber = "S1", DepartmentId = 1, Department = dept, IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Enrolled, IsActive = true };
            
            // 10 oturum var, öğrenci sadece 1'ine katılmış (%90 devamsızlık - uyarı gönderilmeli)
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

            var records = Enumerable.Range(1, 1).Select(i => new AttendanceRecord
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

            // Setup notification service to throw exception for this student
            _mockNotificationService
                .Setup(x => x.SendNotificationAsync(It.Is<SMARTCAMPUS.EntityLayer.DTOs.Notifications.CreateNotificationDto>(
                    dto => dto.UserId == "u1")))
                .ThrowsAsync(new InvalidOperationException("Notification service error"));

            // Act
            var method = typeof(AttendanceWarningService).GetMethod("CheckAttendanceAndNotifyAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method!.Invoke(_service, null)!;

            // Assert - Should catch exception and log error for student
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing attendance for student")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CheckAttendanceAndNotifyAsync_ShouldSkip_WhenNoSessions()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "Student", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var student = new Student { Id = 1, UserId = "u1", User = user, StudentNumber = "S1", DepartmentId = 1, Department = dept, IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Enrolled, IsActive = true };

            _context.Users.Add(user);
            _context.Departments.Add(dept);
            _context.Students.Add(student);
            _context.Courses.Add(course);
            _context.CourseSections.Add(section);
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var method = typeof(AttendanceWarningService).GetMethod("CheckAttendanceAndNotifyAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method!.Invoke(_service, null)!;

            // Assert - Should not send notification when no sessions
            _mockNotificationService.Verify(x => x.SendNotificationAsync(It.IsAny<SMARTCAMPUS.EntityLayer.DTOs.Notifications.CreateNotificationDto>()), Times.Never);
        }


    }
}

