using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SMARTCAMPUS.BusinessLayer.Abstract;
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
        private readonly Mock<IAdvancedNotificationService> _mockNotificationService;

        public AttendanceManagerTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _mockNotificationService = new Mock<IAdvancedNotificationService>();
            _manager = new AttendanceManager(new UnitOfWork(_context), _mockNotificationService.Object);
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
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var session = new AttendanceSession
            {
                Id = 1,
                Section = section,
                SectionId = 1,
                InstructorId = 1,
                Instructor = instructor,
                Status = AttendanceSessionStatus.Open,
                Date = DateTime.UtcNow.Date,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(10, 30, 0),
                Latitude = 0,
                Longitude = 0,
                GeofenceRadius = 15,
                AttendanceRecords = new List<AttendanceRecord>(),
                IsActive = true
            };
            
            await _context.Departments.AddAsync(dept);
            await _context.Users.AddAsync(user);
            await _context.Faculties.AddAsync(instructor);
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
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = dept.Id, Department = dept };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30 };
            await _context.Departments.AddAsync(dept);
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
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "s1", DepartmentId = 1, Department = dept, IsActive = true };
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Section = section, Student = student, Status = EnrollmentStatus.Enrolled, IsActive = true };
            var session = new AttendanceSession { Id = 1, SectionId = 1, Section = section, InstructorId = 1, Instructor = instructor, Date = DateTime.UtcNow.Date, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), Latitude = 0, Longitude = 0, GeofenceRadius = 15, IsActive = true };
            var record = new AttendanceRecord { Id = 1, StudentId = 1, Student = student, SessionId = 1, Session = session, CheckInTime = DateTime.UtcNow, IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Users.AddAsync(user);
            await _context.Faculties.AddAsync(instructor);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.Students.AddAsync(student);
            await _context.Enrollments.AddAsync(enrollment);
            await _context.AttendanceSessions.AddAsync(session);
            await _context.AttendanceRecords.AddAsync(record);
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
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var user = new User { Id = "u1", FullName = "Student", IsActive = true };
            var student = new Student { Id = 1, StudentNumber = "123", UserId = "u1", User = user, DepartmentId = 1, Department = dept, IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructorUser = new User { Id = "u2", FullName = "Instructor", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u2", User = instructorUser, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var session = new AttendanceSession { Id = 1, Section = section, SectionId = 1, InstructorId = 1, Instructor = instructor, Date = DateTime.UtcNow.Date, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), Latitude = 0, Longitude = 0, GeofenceRadius = 15, IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Users.AddRangeAsync(user, instructorUser);
            await _context.Faculties.AddAsync(instructor);
            await _context.Students.AddAsync(student);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.AttendanceSessions.AddAsync(session);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.CreateExcuseRequestAsync(1, new CreateExcuseRequestDto { SessionId = 1, Reason = "Sick" }, "url");

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
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = dept.Id, Department = dept };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30 };
            await _context.Departments.AddAsync(dept);
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
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = dept.Id, Department = dept };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30 };
            await _context.Departments.AddAsync(dept);
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
