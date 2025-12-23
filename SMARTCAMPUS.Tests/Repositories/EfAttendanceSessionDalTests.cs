using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Repositories
{
    public class EfAttendanceSessionDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfAttendanceSessionDal _repository;

        public EfAttendanceSessionDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _repository = new EfAttendanceSessionDal(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetSessionWithRecordsAsync_ShouldReturnSession_WhenExists()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "Instructor" };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "EMP1", Title = "Dr." };
            var course = new Course { Id = 1, Name = "CS101", Code = "CS101" };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            var studentUser = new User { Id = "u2", FullName = "Student" };
            var student = new Student { Id = 1, UserId = "u2", User = studentUser, StudentNumber = "S1" };

            var session = new AttendanceSession
            {
                Id = 1,
                SectionId = 1,
                Section = section,
                InstructorId = 1,
                Instructor = instructor
            };

            var record = new AttendanceRecord { Id = 1, SessionId = 1, StudentId = 1, Student = student };

            await _context.Users.AddRangeAsync(user, studentUser);
            await _context.Faculties.AddAsync(instructor);
            await _context.Students.AddAsync(student);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.AttendanceSessions.AddAsync(session);
            await _context.AttendanceRecords.AddAsync(record);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSessionWithRecordsAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Section.Should().NotBeNull();
            result.Section.Course.Should().NotBeNull();
            result.Instructor.Should().NotBeNull();
            result.Instructor.User.Should().NotBeNull();
            result.AttendanceRecords.Should().HaveCount(1);
            result.AttendanceRecords.First().Student.User.Should().NotBeNull();
        }

        [Fact]
        public async Task GetSessionsBySectionAsync_ShouldReturnSessions_WhenExists()
        {
            // Arrange
            // Create required section to satisfy FK if needed, though InMemory might not enforce FK strictly, it's safer.
            // But here we just need Sessions.
            // However, sessions require SectionId. If we don't add Section, it might fail? No, InMemory is lenient on FKs usually.
            // But validation might fail on something else? No, session only has SectionId foreign key.
            var session1 = new AttendanceSession { Id = 1, SectionId = 1, Date = DateTime.UtcNow.AddDays(-1) };
            var session2 = new AttendanceSession { Id = 2, SectionId = 1, Date = DateTime.UtcNow };

            await _context.AttendanceSessions.AddRangeAsync(session1, session2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSessionsBySectionAsync(1);

            // Assert
            result.Should().HaveCount(2);
            result.First().Id.Should().Be(2); // OrderByDescending Date
        }

        [Fact]
        public async Task GetSessionsByInstructorAsync_ShouldReturnSessions_WhenExists()
        {
            // Arrange
            var course = new Course { Id = 1, Name = "CS101", Code = "CS101" };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            var session = new AttendanceSession { Id = 1, InstructorId = 1, SectionId = 1, Section = section };

            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.AttendanceSessions.AddAsync(session);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSessionsByInstructorAsync(1);

            // Assert
            result.Should().HaveCount(1);
            result.First().Section.Course.Should().NotBeNull();
        }
    }
}
