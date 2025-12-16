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
    public class EfEnrollmentDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfEnrollmentDal _repository;

        public EfEnrollmentDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _repository = new EfEnrollmentDal(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetEnrollmentsByStudentAsync_ShouldReturnEnrollments()
        {
            // Arrange
            // Student requires User
            var user = new User { Id = "u1", FullName = "Student" };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user };

            var instructorUser = new User { Id = "u2", FullName = "Prof" };
            var instructor = new Faculty { Id = 1, UserId = "u2", User = instructorUser, EmployeeNumber = "E1", Title = "Dr." };

            var enrollment = new Enrollment
            {
                Id = 1,
                StudentId = 1,
                Section = new CourseSection
                {
                    SectionNumber = "1",
                    Semester = "Fall",
                    Year = 2024,
                    Course = new Course { Code = "C1", Name = "C1" },
                    Instructor = instructor
                }
            };

            await _context.Users.AddRangeAsync(user, instructorUser);
            await _context.Faculties.AddAsync(instructor);
            await _context.Students.AddAsync(student);
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetEnrollmentsByStudentAsync(1);

            // Assert
            result.Should().HaveCount(1);
            result.First().Section.Course.Should().NotBeNull();
        }

        [Fact]
        public async Task GetEnrollmentsBySectionAsync_ShouldReturnEnrollments()
        {
            // Arrange
            // Student requires UserId and User. User requires FullName (if validated, but usually IdentityUser validation is looser, let's be safe)
            var user = new User { Id = "u1", FullName = "Student" };
            var enrollment = new Enrollment { Id = 1, SectionId = 1, Student = new Student { UserId = "u1", User = user, StudentNumber = "S1" } };

            await _context.Users.AddAsync(user);
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetEnrollmentsBySectionAsync(1);

            // Assert
            result.Should().HaveCount(1);
            result.First().Student.User.Should().NotBeNull();
        }

        [Fact]
        public async Task GetEnrollmentWithDetailsAsync_ShouldReturnDetails()
        {
             // Arrange
            var user = new User { Id = "u1", FullName = "Student" };
            var enrollment = new Enrollment
            {
                Id = 1,
                Student = new Student { UserId = "u1", User = user, StudentNumber = "S1" },
                Section = new CourseSection { SectionNumber = "1", Semester = "Fall", Year = 2024, Course = new Course { Code = "C1", Name = "C1" } }
            };
            await _context.Users.AddAsync(user);

            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetEnrollmentWithDetailsAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Student.User.Should().NotBeNull();
            result.Section.Course.Should().NotBeNull();
        }

        [Fact]
        public async Task HasStudentCompletedCourseAsync_ShouldReturnTrue_WhenCompleted()
        {
             // Arrange
            var enrollment = new Enrollment
            {
                Id = 1,
                StudentId = 1,
                Status = EnrollmentStatus.Completed,
                Section = new CourseSection { CourseId = 1, SectionNumber = "1", Semester = "Fall", Year = 2024 }
            };

            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasStudentCompletedCourseAsync(1, 1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasStudentCompletedCourseAsync_ShouldReturnFalse_WhenNotCompleted()
        {
             // Arrange
            var enrollment = new Enrollment
            {
                Id = 1,
                StudentId = 1,
                Status = EnrollmentStatus.Enrolled,
                Section = new CourseSection { CourseId = 1, SectionNumber = "1", Semester = "Fall", Year = 2024 }
            };

            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasStudentCompletedCourseAsync(1, 1);

            // Assert
            result.Should().BeFalse();
        }
    }
}
