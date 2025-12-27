using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class ReportExportManagerTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly ReportExportManager _manager;

        public ReportExportManagerTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new CampusContext(options);
            _manager = new ReportExportManager(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task ExportStudentListToExcelAsync_ShouldReturnBytes()
        {
            var user = new User { Id = "u1", FullName = "Student", Email = "student@test.com", IsActive = true };
            var department = new Department { Id = 1, Name = "Dept", Code = "D1", IsActive = true };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user, DepartmentId = 1, Department = department, IsActive = true, GPA = 3.0, CGPA = 3.0 };

            _context.Users.Add(user);
            _context.Departments.Add(department);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var result = await _manager.ExportStudentListToExcelAsync();

            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task ExportStudentListToExcelAsync_WithDepartmentId_ShouldReturnBytes()
        {
            var user = new User { Id = "u1", FullName = "Student", Email = "student@test.com", IsActive = true };
            var department = new Department { Id = 1, Name = "Dept", Code = "D1", IsActive = true };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user, DepartmentId = 1, Department = department, IsActive = true };

            _context.Users.Add(user);
            _context.Departments.Add(department);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var result = await _manager.ExportStudentListToExcelAsync(1);

            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task ExportGradeReportToExcelAsync_ShouldReturnBytes()
        {
            var user = new User { Id = "u1", FullName = "Student", Email = "student@test.com", IsActive = true };
            var instructorUser = new User { Id = "u2", FullName = "Instructor", IsActive = true };
            var department = new Department { Id = 1, Name = "Dept", Code = "D1", IsActive = true };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user, DepartmentId = 1, IsActive = true };
            var instructor = new Faculty { Id = 1, EmployeeNumber = "E1", UserId = "u2", User = instructorUser, DepartmentId = 1, Department = department, Title = "Prof", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course", DepartmentId = 1, Credits = 3, ECTS = 5, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Year = 2024, Semester = "Fall", Capacity = 30, IsActive = true };
            var enrollment = new Enrollment { Id = 1, StudentId = 1, Student = student, SectionId = 1, Section = section, MidtermGrade = 70, FinalGrade = 80, LetterGrade = "BB", IsActive = true };

            _context.Users.AddRange(user, instructorUser);
            _context.Departments.Add(department);
            _context.Students.Add(student);
            _context.Faculties.Add(instructor);
            _context.Courses.Add(course);
            _context.CourseSections.Add(section);
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            var result = await _manager.ExportGradeReportToExcelAsync(1);

            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task ExportGradeReportToExcelAsync_ShouldThrow_WhenSectionNotFound()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _manager.ExportGradeReportToExcelAsync(999));
        }

        [Fact]
        public async Task ExportTranscriptToPdfAsync_ShouldReturnBytes()
        {
            var user = new User { Id = "u1", FullName = "Student", Email = "student@test.com", IsActive = true };
            var department = new Department { Id = 1, Name = "Dept", Code = "D1", IsActive = true };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user, DepartmentId = 1, Department = department, IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course", DepartmentId = 1, Credits = 3, ECTS = 5, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "01", Year = 2024, Semester = "Fall", Capacity = 30, IsActive = true };
            var enrollment = new Enrollment { Id = 1, StudentId = 1, Student = student, SectionId = 1, Section = section, LetterGrade = "AA", IsActive = true };

            _context.Users.Add(user);
            _context.Departments.Add(department);
            _context.Students.Add(student);
            _context.Courses.Add(course);
            _context.CourseSections.Add(section);
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            var result = await _manager.ExportTranscriptToPdfAsync(1);

            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task ExportTranscriptToPdfAsync_ShouldThrow_WhenStudentNotFound()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _manager.ExportTranscriptToPdfAsync(999));
        }

        [Fact]
        public async Task ExportAttendanceReportToPdfAsync_ShouldReturnBytes()
        {
            var user = new User { Id = "u1", FullName = "Student", Email = "student@test.com", IsActive = true };
            var instructorUser = new User { Id = "u2", FullName = "Instructor", IsActive = true };
            var department = new Department { Id = 1, Name = "Dept", Code = "D1", IsActive = true };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user, DepartmentId = 1, IsActive = true };
            var instructor = new Faculty { Id = 1, EmployeeNumber = "E1", UserId = "u2", User = instructorUser, DepartmentId = 1, Department = department, Title = "Prof", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course", DepartmentId = 1, Credits = 3, ECTS = 5, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Year = 2024, Semester = "Fall", Capacity = 30, IsActive = true };
            var enrollment = new Enrollment { Id = 1, StudentId = 1, Student = student, SectionId = 1, Section = section, Status = EnrollmentStatus.Enrolled, IsActive = true };
            var session = new AttendanceSession { Id = 1, SectionId = 1, Section = section, InstructorId = 1, Instructor = instructor, Date = DateTime.UtcNow.Date, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(12, 0, 0), Latitude = 0, Longitude = 0, IsActive = true };

            _context.Users.AddRange(user, instructorUser);
            _context.Departments.Add(department);
            _context.Students.Add(student);
            _context.Faculties.Add(instructor);
            _context.Courses.Add(course);
            _context.CourseSections.Add(section);
            _context.Enrollments.Add(enrollment);
            _context.AttendanceSessions.Add(session);
            await _context.SaveChangesAsync();

            var result = await _manager.ExportAttendanceReportToPdfAsync(1);

            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task ExportAtRiskStudentsToExcelAsync_ShouldReturnBytes()
        {
            var user = new User { Id = "u1", FullName = "Student", Email = "student@test.com", IsActive = true };
            var department = new Department { Id = 1, Name = "Dept", Code = "D1", IsActive = true };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user, DepartmentId = 1, Department = department, IsActive = true, GPA = 1.5, CGPA = 1.5 };

            _context.Users.Add(user);
            _context.Departments.Add(department);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var result = await _manager.ExportAtRiskStudentsToExcelAsync();

            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }
    }
}

