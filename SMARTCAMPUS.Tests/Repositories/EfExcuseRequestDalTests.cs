using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Repositories
{
    public class EfExcuseRequestDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfExcuseRequestDal _repository;

        public EfExcuseRequestDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _repository = new EfExcuseRequestDal(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetRequestsByStudentAsync_ShouldReturnRequests()
        {
            // Arrange
            var req = new ExcuseRequest
            {
                Id = 1,
                StudentId = 1,
                Reason = "Reason",
                Session = new AttendanceSession { Section = new CourseSection { SectionNumber = "1", Semester = "Fall", Year = 2024, Course = new Course { Code = "C1", Name = "C1" } } }
            };

            await _context.ExcuseRequests.AddAsync(req);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRequestsByStudentAsync(1);

            // Assert
            result.Should().HaveCount(1);
            result.First().Session.Section.Course.Should().NotBeNull();
        }

        [Fact]
        public async Task GetRequestsBySessionAsync_ShouldReturnRequests()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "Student" };
            var req = new ExcuseRequest
            {
                Id = 1,
                SessionId = 1,
                Reason = "Reason",
                Student = new Student { UserId = "u1", User = user, StudentNumber = "S1" }
            };
            await _context.Users.AddAsync(user);

            await _context.ExcuseRequests.AddAsync(req);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRequestsBySessionAsync(1);

            // Assert
            result.Should().HaveCount(1);
            result.First().Student.User.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPendingRequestsAsync_ShouldReturnPendingOnly()
        {
            // Arrange
            var pending = new ExcuseRequest
            {
                Id = 1,
                Status = ExcuseRequestStatus.Pending,
                Reason = "Reason",
                Student = new Student { User = new User { FullName = "Student" }, StudentNumber = "S1" },
                Session = new AttendanceSession { Section = new CourseSection { SectionNumber = "1", Semester = "Fall", Year = 2024, Course = new Course { Code = "C1", Name = "C1" } } }
            };
            var approved = new ExcuseRequest { Id = 2, Status = ExcuseRequestStatus.Approved, Reason = "R2" };

            await _context.ExcuseRequests.AddRangeAsync(pending, approved);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetPendingRequestsAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(1);
        }
    }
}
