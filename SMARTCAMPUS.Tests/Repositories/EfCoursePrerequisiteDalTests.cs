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
    public class EfCoursePrerequisiteDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfCoursePrerequisiteDal _repository;

        public EfCoursePrerequisiteDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _repository = new EfCoursePrerequisiteDal(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetPrerequisitesForCourseAsync_ShouldReturnPrereqs()
        {
            // Arrange
            var course = new Course { Id = 1, Code = "C1", Name = "C1" };
            var pre1 = new Course { Id = 2, Code = "C2", Name = "C2" };
            var pre2 = new Course { Id = 3, Code = "C3", Name = "C3" };

            await _context.Courses.AddRangeAsync(course, pre1, pre2);
            await _context.CoursePrerequisites.AddRangeAsync(
                new CoursePrerequisite { CourseId = 1, PrerequisiteCourseId = 2 },
                new CoursePrerequisite { CourseId = 1, PrerequisiteCourseId = 3 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetPrerequisitesForCourseAsync(1);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllPrerequisiteIdsRecursiveAsync_ShouldReturnAllLevels()
        {
            // Arrange: 1 -> 2 -> 3
            await _context.Courses.AddRangeAsync(
                new Course { Id = 1, Code = "C1", Name = "C1" }, new Course { Id = 2, Code = "C2", Name = "C2" }, new Course { Id = 3, Code = "C3", Name = "C3" }
            );
            await _context.CoursePrerequisites.AddRangeAsync(
                new CoursePrerequisite { CourseId = 1, PrerequisiteCourseId = 2 },
                new CoursePrerequisite { CourseId = 2, PrerequisiteCourseId = 3 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllPrerequisiteIdsRecursiveAsync(1);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(new[] { 2, 3 });
        }

        [Fact]
        public async Task AddAsync_ShouldAddPrerequisite()
        {
            // Arrange
            var prereq = new CoursePrerequisite { CourseId = 1, PrerequisiteCourseId = 2 };

            // Act
            await _repository.AddAsync(prereq);

            // Assert
            var exists = await _context.CoursePrerequisites.AnyAsync(p => p.CourseId == 1 && p.PrerequisiteCourseId == 2);
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task RemoveAsync_ShouldRemovePrerequisite()
        {
            // Arrange
            var prereq = new CoursePrerequisite { CourseId = 1, PrerequisiteCourseId = 2 };
            await _context.CoursePrerequisites.AddAsync(prereq);
            await _context.SaveChangesAsync();

            // Act
            await _repository.RemoveAsync(1, 2);

            // Assert
            var exists = await _context.CoursePrerequisites.AnyAsync(p => p.CourseId == 1 && p.PrerequisiteCourseId == 2);
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnCorrectly()
        {
            // Arrange
            var prereq = new CoursePrerequisite { CourseId = 1, PrerequisiteCourseId = 2 };
            await _context.CoursePrerequisites.AddAsync(prereq);
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repository.ExistsAsync(1, 2);
            var notExists = await _repository.ExistsAsync(1, 3);

            // Assert
            exists.Should().BeTrue();
            notExists.Should().BeFalse();
        }
    }
}
