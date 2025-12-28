using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class AnalyticsManagerTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly AnalyticsManager _manager;

        public AnalyticsManagerTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new CampusContext(options);
            _manager = new AnalyticsManager(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetDashboardStatsAsync_ShouldReturnSuccess()
        {
            var user = new User { Id = "u1", FullName = "User", IsActive = true };
            var department = new Department { Id = 1, Name = "Dept", Code = "D1", IsActive = true };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user, IsActive = true };
            var faculty = new Faculty { Id = 1, EmployeeNumber = "E1", UserId = "u1", User = user, Title = "Prof", DepartmentId = 1, Department = department, IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course", DepartmentId = 1, Credits = 3, ECTS = 5, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };

            _context.Users.Add(user);
            _context.Students.Add(student);
            _context.Faculties.Add(faculty);
            _context.Departments.Add(department);
            _context.Courses.Add(course);
            _context.CourseSections.Add(section);
            await _context.SaveChangesAsync();

            var result = await _manager.GetDashboardStatsAsync();

            result.IsSuccessful.Should().BeTrue();
            result.Data.TotalUsers.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact(Skip = "LINQ expression cannot be translated in in-memory database")]
        public async Task GetAcademicPerformanceAsync_ShouldReturnSuccess()
        {
            var user = new User { Id = "u1", FullName = "User", IsActive = true };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user, IsActive = true, GPA = 3.0, CGPA = 3.0 };
            var department = new Department { Id = 1, Name = "Dept", Code = "D1", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course", DepartmentId = 1, Credits = 3, ECTS = 5, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, LetterGrade = "AA", IsActive = true };

            _context.Users.Add(user);
            _context.Students.Add(student);
            _context.Departments.Add(department);
            _context.Courses.Add(course);
            _context.CourseSections.Add(section);
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            var result = await _manager.GetAcademicPerformanceAsync();

            result.IsSuccessful.Should().BeTrue();
            result.Data.TotalEnrollments.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact(Skip = "LINQ expression cannot be translated in in-memory database")]
        public async Task GetDepartmentGpaStatsAsync_ShouldReturnSuccess()
        {
            var user = new User { Id = "u1", FullName = "User", IsActive = true };
            var department = new Department { Id = 1, Name = "Dept", Code = "D1", IsActive = true };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user, DepartmentId = 1, IsActive = true, GPA = 3.0, CGPA = 3.0 };

            _context.Users.Add(user);
            _context.Departments.Add(department);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var result = await _manager.GetDepartmentGpaStatsAsync();

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCountGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetDepartmentStatsAsync_ShouldReturnSuccess_WhenExists()
        {
            var user = new User { Id = "u1", FullName = "User", IsActive = true };
            var department = new Department { Id = 1, Name = "Dept", Code = "D1", IsActive = true };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user, DepartmentId = 1, IsActive = true, GPA = 3.0, CGPA = 3.0 };

            _context.Users.Add(user);
            _context.Departments.Add(department);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var result = await _manager.GetDepartmentStatsAsync(1);

            result.IsSuccessful.Should().BeTrue();
            result.Data.DepartmentId.Should().Be(1);
        }

        [Fact]
        public async Task GetDepartmentStatsAsync_ShouldReturnFail_WhenNotFound()
        {
            var result = await _manager.GetDepartmentStatsAsync(999);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetGradeDistributionAsync_ShouldReturnSuccess()
        {
            var dept = new Department { Id = 1, Name = "Dept", Code = "D1", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course", DepartmentId = 1, Credits = 3, ECTS = 5, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var enrollment = new Enrollment { Id = 1, SectionId = 1, LetterGrade = "AA", IsActive = true };

            _context.Departments.Add(dept);
            _context.Courses.Add(course);
            _context.CourseSections.Add(section);
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            var result = await _manager.GetGradeDistributionAsync();

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCountGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetAtRiskStudentsAsync_ShouldReturnSuccess()
        {
            var user = new User { Id = "u1", FullName = "User", IsActive = true };
            var department = new Department { Id = 1, Name = "Dept", Code = "D1", IsActive = true };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user, DepartmentId = 1, IsActive = true, GPA = 1.5, CGPA = 1.5 };

            _context.Users.Add(user);
            _context.Departments.Add(department);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var result = await _manager.GetAtRiskStudentsAsync();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task GetCourseOccupancyAsync_ShouldReturnSuccess()
        {
            var dept = new Department { Id = 1, Name = "Dept", Code = "D1", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course", DepartmentId = 1, Credits = 3, ECTS = 5, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };

            _context.Departments.Add(dept);
            _context.Courses.Add(course);
            _context.CourseSections.Add(section);
            await _context.SaveChangesAsync();

            var result = await _manager.GetCourseOccupancyAsync();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task GetAttendanceStatsAsync_ShouldReturnSuccess()
        {
            var course = new Course { Id = 1, Code = "C1", Name = "Course", DepartmentId = 1, Credits = 3, ECTS = 5, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var session = new AttendanceSession { Id = 1, SectionId = 1, Date = DateTime.UtcNow.Date, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), Latitude = 0, Longitude = 0, IsActive = true };

            _context.Courses.Add(course);
            _context.CourseSections.Add(section);
            _context.AttendanceSessions.Add(session);
            await _context.SaveChangesAsync();

            var result = await _manager.GetAttendanceStatsAsync();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task GetGradeDistributionAsync_ShouldReturnSuccess_WithSectionId()
        {
            var dept = new Department { Id = 1, Name = "Dept", Code = "D1", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course", DepartmentId = 1, Credits = 3, ECTS = 5, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var enrollment = new Enrollment { Id = 1, SectionId = 1, LetterGrade = "AA", IsActive = true };

            _context.Departments.Add(dept);
            _context.Courses.Add(course);
            _context.CourseSections.Add(section);
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            var result = await _manager.GetGradeDistributionAsync(1);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCountGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetAtRiskStudentsAsync_ShouldReturnSuccess_WithCustomThresholds()
        {
            var user = new User { Id = "u1", FullName = "User", IsActive = true };
            var department = new Department { Id = 1, Name = "Dept", Code = "D1", IsActive = true };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user, DepartmentId = 1, IsActive = true, GPA = 1.5, CGPA = 1.5 };

            _context.Users.Add(user);
            _context.Departments.Add(department);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var result = await _manager.GetAtRiskStudentsAsync(2.0, 20.0);

            result.IsSuccessful.Should().BeTrue();
        }
    }
}

