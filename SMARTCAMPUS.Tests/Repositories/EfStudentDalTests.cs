using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Repositories
{
    public class EfStudentDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfStudentDal _repository;

        public EfStudentDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _repository = new EfStudentDal(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetStudentWithDetailsAsync_ShouldReturnStudent_WhenExistsAndActive()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var user = new User { Id = "u1", FullName = "Test Student" };
            var student = new Student
            {
                Id = 1,
                StudentNumber = "S1",
                UserId = "u1",
                User = user,
                DepartmentId = 1,
                Department = dept,
                IsActive = true
            };

            await _context.Departments.AddAsync(dept);
            await _context.Users.AddAsync(user);
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetStudentWithDetailsAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.User.Should().NotBeNull();
            result.Department.Should().NotBeNull();
        }

        [Fact]
        public async Task GetStudentWithDetailsAsync_ShouldReturnNull_WhenInactive()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var user = new User { Id = "u1", FullName = "Test Student" };
            var student = new Student
            {
                Id = 1,
                StudentNumber = "S1",
                UserId = "u1",
                User = user,
                DepartmentId = 1,
                Department = dept,
                IsActive = false
            };

            await _context.Departments.AddAsync(dept);
            await _context.Users.AddAsync(user);
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetStudentWithDetailsAsync(1);

            // Assert
            result.Should().BeNull();
        }
    }
}
