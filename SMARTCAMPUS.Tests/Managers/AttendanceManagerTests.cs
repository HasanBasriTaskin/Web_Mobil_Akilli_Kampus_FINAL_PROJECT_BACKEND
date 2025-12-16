using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs.Attendance;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using SMARTCAMPUS.Tests.TestUtilities;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class AttendanceManagerTests : IDisposable
    {
        private readonly Mock<IAttendanceSessionDal> _mockSessionDal;
        private readonly Mock<IAttendanceRecordDal> _mockRecordDal;
        private readonly Mock<IExcuseRequestDal> _mockExcuseDal;
        private readonly CampusContext _context;
        private readonly AttendanceManager _manager;

        public AttendanceManagerTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _mockSessionDal = new Mock<IAttendanceSessionDal>();
            _mockRecordDal = new Mock<IAttendanceRecordDal>();
            _mockExcuseDal = new Mock<IExcuseRequestDal>();

            _manager = new AttendanceManager(_mockSessionDal.Object, _mockRecordDal.Object, _mockExcuseDal.Object, _context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region CreateSessionAsync Tests

        [Fact]
        public async Task CreateSessionAsync_ShouldFail_WhenSectionNotFound()
        {
            // Arrange
            var dto = new CreateSessionDto { SectionId = 1 };

            // Act
            var result = await _manager.CreateSessionAsync(1, dto);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task CreateSessionAsync_ShouldSucceed_WhenValid()
        {
            // Arrange
            var course = new Course { Id = 1, Code = "C1", Name = "C1" };
            var section = new CourseSection { Id = 1, InstructorId = 1, CourseId = 1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();

            var dto = new CreateSessionDto { SectionId = 1, Date = DateTime.UtcNow, StartTime = TimeSpan.Zero, EndTime = TimeSpan.FromHours(1) };

            _mockSessionDal.Setup(x => x.AddAsync(It.IsAny<AttendanceSession>())).Returns(Task.CompletedTask);

            // Act
            var result = await _manager.CreateSessionAsync(1, dto);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
            _mockSessionDal.Verify(x => x.AddAsync(It.IsAny<AttendanceSession>()), Times.Once);
        }

        #endregion

        #region GetSessionByIdAsync Tests

        [Fact]
        public async Task GetSessionByIdAsync_ShouldFail_WhenSessionNotFound()
        {
            // Arrange
            _mockSessionDal.Setup(x => x.GetSessionWithRecordsAsync(1)).ReturnsAsync((AttendanceSession?)null);

            // Act
            var result = await _manager.GetSessionByIdAsync(1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetSessionByIdAsync_ShouldSucceed_WhenExists()
        {
            // Arrange
            var session = new AttendanceSession
            {
                Id = 1,
                Section = new CourseSection { Course = new Course { Code = "C1", Name = "C1" }, SectionNumber = "1" },
                AttendanceRecords = new List<AttendanceRecord>()
            };
            _mockSessionDal.Setup(x => x.GetSessionWithRecordsAsync(1)).ReturnsAsync(session);

            // Act
            var result = await _manager.GetSessionByIdAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.CourseCode.Should().Be("C1");
        }

        #endregion

        #region CloseSessionAsync Tests

        [Fact]
        public async Task CloseSessionAsync_ShouldFail_WhenSessionNotFound()
        {
            // Act
            var result = await _manager.CloseSessionAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task CloseSessionAsync_ShouldSucceed_WhenOpen()
        {
            // Arrange
            var session = new AttendanceSession { Id = 1, InstructorId = 1, Status = AttendanceSessionStatus.Open };
            await _context.AttendanceSessions.AddAsync(session);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.CloseSessionAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            session.Status.Should().Be(AttendanceSessionStatus.Closed);
            _mockSessionDal.Verify(x => x.Update(session), Times.Once);
        }

        #endregion

        #region CheckInAsync Tests

        [Fact]
        public async Task CheckInAsync_ShouldFail_WhenSessionNotFound()
        {
            // Act
            var result = await _manager.CheckInAsync(1, 1, new CheckInDto());

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task CheckInAsync_ShouldFail_WhenNotEnrolled()
        {
            // Arrange
            var session = new AttendanceSession { Id = 1, SectionId = 1, Status = AttendanceSessionStatus.Open };
            await _context.AttendanceSessions.AddAsync(session);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.CheckInAsync(1, 1, new CheckInDto());

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task CheckInAsync_ShouldSucceed_WhenValid()
        {
            // Arrange
            var session = new AttendanceSession { Id = 1, SectionId = 1, Status = AttendanceSessionStatus.Open, Latitude = 0, Longitude = 0, GeofenceRadius = 100 };
            await _context.AttendanceSessions.AddAsync(session);

            var enrollment = new Enrollment { StudentId = 1, SectionId = 1, Status = EnrollmentStatus.Enrolled };
            await _context.Enrollments.AddAsync(enrollment);

            await _context.SaveChangesAsync();

            _mockRecordDal.Setup(x => x.HasStudentCheckedInAsync(1, 1)).ReturnsAsync(false);
            _mockRecordDal.Setup(x => x.AddAsync(It.IsAny<AttendanceRecord>())).Returns(Task.CompletedTask);

            // Act
            var result = await _manager.CheckInAsync(1, 1, new CheckInDto { Latitude = 0, Longitude = 0 });

            // Assert
            result.IsSuccessful.Should().BeTrue();
            _mockRecordDal.Verify(x => x.AddAsync(It.IsAny<AttendanceRecord>()), Times.Once);
        }

        #endregion

        #region GetMyAttendanceAsync Tests

        [Fact]
        public async Task GetMyAttendanceAsync_ShouldReturnAttendance()
        {
            // Arrange
            var course = new Course { Id = 1, Code = "C1", Name = "C1" };
            var section = new CourseSection { Id = 1, Course = course, SectionNumber = "1", Semester = "F", Year = 2024 };
            var enrollment = new Enrollment { StudentId = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Enrolled };

            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetMyAttendanceAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data.First().CourseCode.Should().Be("C1");
        }

        #endregion

        #region CreateExcuseRequestAsync Tests

        [Fact]
        public async Task CreateExcuseRequestAsync_ShouldSucceed()
        {
            // Arrange
            var course = new Course { Id = 1, Code = "C1", Name = "C1" };
            var section = new CourseSection { Id = 1, Course = course, SectionNumber = "1", Semester = "F", Year = 2024 };
            var session = new AttendanceSession { Id = 1, Section = section };

            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.AttendanceSessions.AddAsync(session);
            await _context.SaveChangesAsync();

            _mockExcuseDal.Setup(x => x.AddAsync(It.IsAny<ExcuseRequest>())).Returns(Task.CompletedTask);

            // Act
            var result = await _manager.CreateExcuseRequestAsync(1, new CreateExcuseRequestDto { SessionId = 1, Reason = "Sick" }, "url");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
            _mockExcuseDal.Verify(x => x.AddAsync(It.IsAny<ExcuseRequest>()), Times.Once);
        }

        #endregion

        #region GetMySessionsAsync Tests

        [Fact]
        public async Task GetMySessionsAsync_ShouldReturnSessions()
        {
            // Arrange
            var sessions = new List<AttendanceSession>
            {
                new AttendanceSession { Id = 1, Section = new CourseSection { Course = new Course { Code = "C1", Name = "C1" }, SectionNumber = "1" } }
            };
            _mockSessionDal.Setup(x => x.GetSessionsByInstructorAsync(1)).ReturnsAsync(sessions);

            // Act
            var result = await _manager.GetMySessionsAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        #endregion

        #region GetSessionRecordsAsync Tests

        [Fact]
        public async Task GetSessionRecordsAsync_ShouldReturnRecords()
        {
            // Arrange
            var records = new List<AttendanceRecord>
            {
                new AttendanceRecord { Id = 1, Student = new Student { StudentNumber = "S1", User = new User { FullName = "Student" } } }
            };
            _mockRecordDal.Setup(x => x.GetRecordsBySessionAsync(1)).ReturnsAsync(records);

            // Act
            var result = await _manager.GetSessionRecordsAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        #endregion

        #region GetExcuseRequestsAsync Tests

        [Fact]
        public async Task GetExcuseRequestsAsync_ShouldReturnRequests()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "Student" };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user };
            var course = new Course { Id = 1, Code = "C1", Name = "C1" };
            var section = new CourseSection { Id = 1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            var session = new AttendanceSession { Id = 1, Section = section, InstructorId = 1 };

            var request = new ExcuseRequest
            {
                Id = 1,
                StudentId = 1,
                Student = student,
                SessionId = 1,
                Session = session,
                Reason = "Sick",
                CreatedDate = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.Students.AddAsync(student);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.AttendanceSessions.AddAsync(session);
            await _context.ExcuseRequests.AddAsync(request);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetExcuseRequestsAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        #endregion

        #region ApproveExcuseRequestAsync Tests

        [Fact]
        public async Task ApproveExcuseRequestAsync_ShouldSucceed()
        {
            // Arrange
            var session = new AttendanceSession { Id = 1, InstructorId = 1 };
            var request = new ExcuseRequest { Id = 1, SessionId = 1, Session = session, Status = ExcuseRequestStatus.Pending, Reason = "Sick" };

            await _context.AttendanceSessions.AddAsync(session);
            await _context.ExcuseRequests.AddAsync(request);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.ApproveExcuseRequestAsync(1, 1, new ReviewExcuseRequestDto { Notes = "Ok" });

            // Assert
            result.IsSuccessful.Should().BeTrue();
            request.Status.Should().Be(ExcuseRequestStatus.Approved);
            _mockExcuseDal.Verify(x => x.Update(request), Times.Once);
        }

        #endregion

        #region RejectExcuseRequestAsync Tests

        [Fact]
        public async Task RejectExcuseRequestAsync_ShouldSucceed()
        {
            // Arrange
            var session = new AttendanceSession { Id = 1, InstructorId = 1 };
            var request = new ExcuseRequest { Id = 1, SessionId = 1, Session = session, Status = ExcuseRequestStatus.Pending, Reason = "Sick" };

            await _context.AttendanceSessions.AddAsync(session);
            await _context.ExcuseRequests.AddAsync(request);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.RejectExcuseRequestAsync(1, 1, new ReviewExcuseRequestDto { Notes = "No" });

            // Assert
            result.IsSuccessful.Should().BeTrue();
            request.Status.Should().Be(ExcuseRequestStatus.Rejected);
            _mockExcuseDal.Verify(x => x.Update(request), Times.Once);
        }

        #endregion
    }
}
