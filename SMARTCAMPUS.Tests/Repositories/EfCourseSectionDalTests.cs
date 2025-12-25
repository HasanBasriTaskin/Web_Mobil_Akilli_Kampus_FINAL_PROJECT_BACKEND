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
    public class EfCourseSectionDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfCourseSectionDal _repository;

        public EfCourseSectionDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _repository = new EfCourseSectionDal(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetSectionWithDetailsAsync_ShouldReturnSection()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "Prof" };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "EMP1", Title = "Dr." };
            var course = new Course { Id = 1, Name = "C1", Code = "C1" };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024 };

            await _context.Users.AddAsync(user);
            await _context.Faculties.AddAsync(instructor);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSectionWithDetailsAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Course.Should().NotBeNull();
            result.Instructor.Should().NotBeNull();
            result.Instructor.User.Should().NotBeNull();
        }

        [Fact]
        public async Task GetSectionsByInstructorAsync_ShouldReturnInstructorSections()
        {
            // Arrange
            var user1 = new User { Id = "u1", FullName = "Prof 1" };
            var instructor1 = new Faculty { Id = 1, UserId = "u1", User = user1, EmployeeNumber = "E1", Title = "Dr." };
            var user2 = new User { Id = "u2", FullName = "Prof 2" };
            var instructor2 = new Faculty { Id = 2, UserId = "u2", User = user2, EmployeeNumber = "E2", Title = "Dr." };
            var course = new Course { Id = 1, Code = "C1", Name = "C1" };

            var section1 = new CourseSection { Id = 1, InstructorId = 1, Instructor = instructor1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            var section2 = new CourseSection { Id = 2, InstructorId = 2, Instructor = instructor2, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024 };

            await _context.Users.AddRangeAsync(user1, user2);
            await _context.Faculties.AddRangeAsync(instructor1, instructor2);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddRangeAsync(section1, section2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSectionsByInstructorAsync(1);

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(1);
        }

        [Fact]
        public async Task IncrementEnrolledCountAsync_InMemory_ShouldWorkCorrectly()
        {
            // The DAL now detects InMemory provider and uses Find/Update instead of raw SQL
            // Arrange
            var section = new CourseSection { Id = 1, SectionNumber = "S1", Semester = "Fall", Year = 2024, Capacity = 30, EnrolledCount = 5 };
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            // Act
            await _repository.IncrementEnrolledCountAsync(1);

            // Assert
            var updated = await _context.CourseSections.FindAsync(1);
            updated!.EnrolledCount.Should().Be(6);
        }

        // Since InMemory now works, let's also test Decrement
        [Fact]
        public async Task DecrementEnrolledCountAsync_InMemory_ShouldWorkCorrectly()
        {
            // Arrange
            var section = new CourseSection { Id = 1, SectionNumber = "S1", Semester = "Fall", Year = 2024, Capacity = 30, EnrolledCount = 5 };
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            // Act
            await _repository.DecrementEnrolledCountAsync(1);

            // Assert
            var updated = await _context.CourseSections.FindAsync(1);
            updated!.EnrolledCount.Should().Be(4);
        }
    }
}
