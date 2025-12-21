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
    public class EfCourseDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfCourseDal _repository;

        public EfCourseDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _repository = new EfCourseDal(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetCourseWithPrerequisitesAsync_ShouldReturnCourse_WhenExists()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var course = new Course { Id = 1, DepartmentId = 1, Department = dept, Name = "Advanced CS", Code = "CS300" };
            var preCourse = new Course { Id = 2, Name = "Intro CS", Code = "CS101" };
            var prerequisite = new CoursePrerequisite { CourseId = 1, PrerequisiteCourseId = 2, PrerequisiteCourse = preCourse };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddRangeAsync(course, preCourse);
            await _context.CoursePrerequisites.AddAsync(prerequisite);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCourseWithPrerequisitesAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Department.Should().NotBeNull();
            result.Prerequisites.Should().HaveCount(1);
            result.Prerequisites.First().PrerequisiteCourse.Should().NotBeNull();
        }

        [Fact]
        public async Task GetCoursesByDepartmentAsync_ShouldReturnActiveCourses()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var activeCourse = new Course { Id = 1, DepartmentId = 1, IsActive = true, Department = dept, Name = "Active", Code = "C1" };
            var inactiveCourse = new Course { Id = 2, DepartmentId = 1, IsActive = false, Department = dept, Name = "Inactive", Code = "C2" };
            var otherDeptCourse = new Course { Id = 3, DepartmentId = 2, IsActive = true, Name = "Other", Code = "C3" };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddRangeAsync(activeCourse, inactiveCourse, otherDeptCourse);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCoursesByDepartmentAsync(1);

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(1);
            result.First().Department.Should().NotBeNull();
        }
    }
}
