using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs.Attendance;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class AttendanceManagerTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly AttendanceManager _manager;

        public AttendanceManagerTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _manager = new AttendanceManager(new UnitOfWork(_context));
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
            // Arrange - Need to seed all required entities with proper FK relationships
            var user = new User { Id = "u1", FullName = "Professor Name", Email = "prof@test.com", UserName = "prof1" };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var faculty = new Faculty { Id = 1, UserId = "u1", User = user, Title = "Prof", EmployeeNumber = "EMP001", DepartmentId = 1, Department = dept };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, DepartmentId = 1 };
            var section = new CourseSection 
            { 
                Id = 1, 
                InstructorId = 1, 
                Instructor = faculty,
                CourseId = 1, 
                Course = course, 
                SectionNumber = "1", 
                Semester = "Fall", 
                Year = 2024 
            };
            
            await _context.Users.AddAsync(user);
            await _context.Departments.AddAsync(dept);
            await _context.Faculties.AddAsync(faculty);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();

            var dto = new CreateSessionDto { SectionId = 1, Date = DateTime.UtcNow, StartTime = TimeSpan.Zero, EndTime = TimeSpan.FromHours(1) };

            // Act
            var result = await _manager.CreateSessionAsync(1, dto);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
            
            var session = await _context.AttendanceSessions.FirstOrDefaultAsync();
            session.Should().NotBeNull();
            session!.SectionId.Should().Be(1);
        }

        #endregion

        #region GetSessionByIdAsync Tests

        [Fact]
        public async Task GetSessionByIdAsync_ShouldFail_WhenSessionNotFound()
        {
            // Act
            var result = await _manager.GetSessionByIdAsync(1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetSessionByIdAsync_ShouldSucceed_WhenExists()
        {
            // Arrange - Need Course, Section with required fields, and Session
            var course = new Course { Id = 1, Code = "C1", Name = "C1" };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            var session = new AttendanceSession
            {
                Id = 1,
                Section = section,
                SectionId = 1,
                Status = AttendanceSessionStatus.Open,
                Date = DateTime.UtcNow
            };
            
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.AttendanceSessions.AddAsync(session);
            await _context.SaveChangesAsync();

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
            // Arrange - Need CourseSection for FK constraint
            var course = new Course { Id = 1, Code = "C1", Name = "C1" };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            var session = new AttendanceSession { Id = 1, InstructorId = 1, Status = AttendanceSessionStatus.Open, SectionId = 1, Section = section, Date = DateTime.UtcNow };
            
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.AttendanceSessions.AddAsync(session);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.CloseSessionAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            var dbSession = await _context.AttendanceSessions.FindAsync(1);
            dbSession!.Status.Should().Be(AttendanceSessionStatus.Closed);
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
            var session = new AttendanceSession { Id = 1, SectionId = 1, Status = AttendanceSessionStatus.Open, Latitude = 0, Longitude = 0, GeofenceRadius = 100, StartTime = TimeSpan.Zero, EndTime = TimeSpan.FromHours(24) };
            // Ensure session is active time-wise if logic checks it. 
            // AttendanceManager logic: if (session.Status != Open) return fail. 
            // Also geolocation check.
            
            await _context.AttendanceSessions.AddAsync(session);

            var enrollment = new Enrollment { StudentId = 1, SectionId = 1, Status = EnrollmentStatus.Enrolled };
            await _context.Enrollments.AddAsync(enrollment);

            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.CheckInAsync(1, 1, new CheckInDto { Latitude = 0, Longitude = 0 });

            // Assert
            result.IsSuccessful.Should().BeTrue();
            var record = await _context.AttendanceRecords.FirstOrDefaultAsync(r => r.StudentId == 1 && r.SessionId == 1);
            record.Should().NotBeNull();
        }

        #endregion

        #region GetMyAttendanceAsync Tests

        [Fact]
        public async Task GetMyAttendanceAsync_ShouldReturnAttendance()
        {
            // Arrange
            // Needs User -> Student -> Enrollment -> Section -> Course
            // Manager: GetUserAsync(User) -> GetByUserId -> StudentId -> ...
            // We need to Mock UserManager? 
            // Wait, AttendanceManager DOES NOT use UserManager directly anymore?
            // Let's check. 
            // Refactoring: "var user = await _userManager.GetUserAsync(User);" line 258.
            // AttendanceManager DOES use UserManager!
            // I removed _context, but _userManager is still there?
            // I need to check if I removed _userManager.
            // If I did NOT remove _userManager, checking GetMyAttendanceAsync in tests is tricky because _userManager is usually Mocked.
            // But I initialized AttendanceManager with `new AttendanceManager(new UnitOfWork(_context))`. 
            // Does AttendanceManager constructor take UserManager?
            // Refactoring Step 325 or so said: "Constructor now injects IUnitOfWork only."
            // If I removed UserManager injection, how does it get current user?
            // Maybe it takes ClaimsPrincipal? No, usually Service doesn't take ClaimsPrincipal directly.
            // Usually Controller passes userId.
            // Check `AttendanceManager` signature.
            // Methods like `GetMyAttendanceAsync(int studentId)`?
            // If the method signature takes `studentId`, good.
            // Old Test `GetMyAttendanceAsync` passed `1`. `GetMyAttendanceAsync(1)`.
            // So it takes `studentId`.
            // So no UserManager needed?
            // Wait, old test `GetMyAttendanceAsync` calls `_manager.GetMyAttendanceAsync(1)`.
            // Code view (Step 358) showed: `r.StudentId == studentId`.
            // So it relies on the argument.
            // Excellent.
            // Need Course and Section with proper FKs for GetMyAttendanceAsync query
            var course = new Course { Id = 1, Code = "C1", Name = "C1" };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1" };
            var enrollment = new Enrollment { StudentId = 1, SectionId = 1, Section = section, Student = student, Status = EnrollmentStatus.Enrolled };

            await _context.Courses.AddAsync(course);
            await _context.Students.AddAsync(student);
            await _context.CourseSections.AddAsync(section);
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetMyAttendanceAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCountGreaterOrEqualTo(1);
        }

        #endregion

        #region CreateExcuseRequestAsync Tests

        [Fact]
        public async Task CreateExcuseRequestAsync_ShouldSucceed()
        {
            // Arrange
            var course = new Course { Id = 1, Code = "C1", Name = "C1" };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            var session = new AttendanceSession { Id = 1, Section = section, SectionId = 1, Date = DateTime.UtcNow.Date };
            
            // Need Student?
            // CreateExcuseRequestAsync logic might check Student existence or Enrollment?
            // Step 325: "GetStudentWithDetailsAsync(studentId)".
            // So we need Student in DB.
            var student = new Student { Id = 1, StudentNumber = "123", UserId = "u1" };

            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.AttendanceSessions.AddAsync(session);
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.CreateExcuseRequestAsync(1, new CreateExcuseRequestDto { SessionId = session.Id, Reason = "Sick" }, "url");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
            
            var req = await _context.ExcuseRequests.FirstOrDefaultAsync();
            req.Should().NotBeNull();
            req!.Reason.Should().Be("Sick");
        }

        #endregion

        #region GetMySessionsAsync Tests

        [Fact]
        public async Task GetMySessionsAsync_ShouldReturnSessions()
        {
            // Arrange
            var course = new Course { Id = 1, Code = "C1", Name = "C1" };
            var section = new CourseSection { Id = 1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            var session = new AttendanceSession { Id = 1, Section = section, SectionId = 1, InstructorId = 1 };
            
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.AttendanceSessions.AddAsync(session);
            await _context.SaveChangesAsync();

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
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1" };
            var user = new User { Id = "u1", FullName = "Student" }; // Student -> User
            student.User = user;
            
            var record = new AttendanceRecord { Id = 1, StudentId = 1, Student = student, SessionId = 1 };
            
            await _context.Users.AddAsync(user);
            await _context.Students.AddAsync(student);
            await _context.AttendanceRecords.AddAsync(record);
            await _context.SaveChangesAsync();

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
            var user = new User { Id = "u1", FullName = "Student" };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user };
            // Need session to verify instructor permission
            var session = new AttendanceSession { Id = 1, InstructorId = 1 };
            var request = new ExcuseRequest 
            { 
                Id = 1, 
                SessionId = 1, 
                Session = session, 
                StudentId = 1,
                Student = student,
                Status = ExcuseRequestStatus.Pending, 
                Reason = "Sick" 
            };

            await _context.Users.AddAsync(user);
            await _context.Students.AddAsync(student);
            await _context.AttendanceSessions.AddAsync(session);
            await _context.ExcuseRequests.AddAsync(request);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.ApproveExcuseRequestAsync(1, 1, new ReviewExcuseRequestDto { Notes = "Ok" });

            // Assert
            result.IsSuccessful.Should().BeTrue();
            var dbReq = await _context.ExcuseRequests.FindAsync(1);
            dbReq!.Status.Should().Be(ExcuseRequestStatus.Approved);
        }

        #endregion

        #region RejectExcuseRequestAsync Tests

        [Fact]
        public async Task RejectExcuseRequestAsync_ShouldSucceed()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "Student" };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user };
            var session = new AttendanceSession { Id = 1, InstructorId = 1 };
            var request = new ExcuseRequest 
            { 
                Id = 1, 
                SessionId = 1, 
                Session = session, 
                StudentId = 1,
                Student = student,
                Status = ExcuseRequestStatus.Pending, 
                Reason = "Sick" 
            };

            await _context.Users.AddAsync(user);
            await _context.Students.AddAsync(student);
            await _context.AttendanceSessions.AddAsync(session);
            await _context.ExcuseRequests.AddAsync(request);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.RejectExcuseRequestAsync(1, 1, new ReviewExcuseRequestDto { Notes = "No" });

            // Assert
            result.IsSuccessful.Should().BeTrue();
            var dbReq = await _context.ExcuseRequests.FindAsync(1);
            dbReq!.Status.Should().Be(ExcuseRequestStatus.Rejected);
        }

        #endregion
    }
}
