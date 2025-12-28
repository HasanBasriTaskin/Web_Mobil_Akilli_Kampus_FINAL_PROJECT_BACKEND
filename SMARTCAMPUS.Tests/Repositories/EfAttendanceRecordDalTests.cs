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
    public class EfAttendanceRecordDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfAttendanceRecordDal _repository;

        public EfAttendanceRecordDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _repository = new EfAttendanceRecordDal(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetRecordsByStudentAsync_ShouldReturnRecords_WhenExists()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "Test Student" };
            var student = new Student { Id = 1, UserId = "u1", User = user, StudentNumber = "123" };
            var course = new Course { Id = 1, Name = "Math", Code = "M101" };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            var session = new AttendanceSession { Id = 1, SectionId = 1, Section = section };
            var record = new AttendanceRecord
            {
                Id = 1,
                StudentId = 1,
                SessionId = 1,
                CheckInTime = DateTime.UtcNow,
                Session = session,
                Student = student
            };

            await _context.Users.AddAsync(user);
            await _context.Students.AddAsync(student);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.AttendanceSessions.AddAsync(session);
            await _context.AttendanceRecords.AddAsync(record);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRecordsByStudentAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            var resultRecord = result.First();
            resultRecord.Session.Should().NotBeNull();
            resultRecord.Session.Section.Should().NotBeNull();
            resultRecord.Session.Section.Course.Should().NotBeNull();
        }

        [Fact]
        public async Task GetRecordsBySessionAsync_ShouldReturnRecords_WhenExists()
        {
             // Arrange
            var user = new User { Id = "u1", FullName = "Test Student" };
            var student = new Student { Id = 1, UserId = "u1", User = user, StudentNumber = "123" };
            var session = new AttendanceSession { Id = 1 };
            var record = new AttendanceRecord
            {
                Id = 1,
                StudentId = 1,
                SessionId = 1,
                CheckInTime = DateTime.UtcNow,
                Student = student
            };

            await _context.Users.AddAsync(user);
            await _context.Students.AddAsync(student);
            await _context.AttendanceSessions.AddAsync(session);
            await _context.AttendanceRecords.AddAsync(record);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRecordsBySessionAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            var resultRecord = result.First();
            resultRecord.Student.Should().NotBeNull();
            resultRecord.Student.User.Should().NotBeNull();
        }

        [Fact]
        public async Task HasStudentCheckedInAsync_ShouldReturnTrue_WhenExists()
        {
             // Arrange
            var record = new AttendanceRecord
            {
                Id = 1,
                StudentId = 1,
                SessionId = 1
            };

            await _context.AttendanceRecords.AddAsync(record);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasStudentCheckedInAsync(1, 1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasStudentCheckedInAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Act
            var result = await _repository.HasStudentCheckedInAsync(1, 1);

            // Assert
            result.Should().BeFalse();
        }
    }
}
