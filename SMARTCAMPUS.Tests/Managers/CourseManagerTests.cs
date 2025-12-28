using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs.Course;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class CourseManagerTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly CourseManager _manager;

        public CourseManagerTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _manager = new CourseManager(new UnitOfWork(_context));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region CreateCourseAsync Tests

        [Fact]
        public async Task CreateCourseAsync_ShouldFail_WhenCodeExists()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var existing = new Course { Code = "C1", Name = "Existing", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept };
            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(existing);
            await _context.SaveChangesAsync();

            var dto = new CreateCourseDto { Code = "C1", Name = "New" };

            // Act
            var result = await _manager.CreateCourseAsync(dto);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task CreateCourseAsync_ShouldSucceed_WhenValid()
        {
            // Arrange
            var dto = new CreateCourseDto { Code = "C2", Name = "New", DepartmentId = 1, Credits = 3, ECTS = 5 };

            // Act
            var result = await _manager.CreateCourseAsync(dto);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Code == "C2");
            course.Should().NotBeNull();
        }

        #endregion

        #region GetCourseByIdAsync Tests

        [Fact]
        public async Task GetCourseByIdAsync_ShouldFail_WhenNotFound()
        {
            // Act
            var result = await _manager.GetCourseByIdAsync(999);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetCourseByIdAsync_ShouldSucceed_WhenExists()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept };
            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetCourseByIdAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Code.Should().Be("C1");
        }

        #endregion

        #region GetAllCoursesAsync Tests

        [Fact]
        public async Task GetAllCoursesAsync_ShouldReturnPagedResults()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var c1 = new Course { Id = 1, Code = "A", Name = "A", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept };
            var c2 = new Course { Id = 2, Code = "B", Name = "B", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept };
            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddRangeAsync(c1, c2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetAllCoursesAsync(1, 10);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllCoursesAsync_ShouldFilterByDepartment()
        {
            // Arrange
            var dept1 = new Department { Id = 1, Name = "CS", Code = "CS" };
            var dept2 = new Department { Id = 2, Name = "Math", Code = "MTH" };
            var c1 = new Course { Id = 1, Code = "A", Name = "A", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept1 };
            var c2 = new Course { Id = 2, Code = "B", Name = "B", Credits = 3, ECTS = 5, DepartmentId = 2, Department = dept2 };

            await _context.Departments.AddRangeAsync(dept1, dept2);
            await _context.Courses.AddRangeAsync(c1, c2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetAllCoursesAsync(1, 10, departmentId: 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data.First().Code.Should().Be("A");
        }

        #endregion

        #region UpdateCourseAsync Tests

        [Fact]
        public async Task UpdateCourseAsync_ShouldFail_WhenNotFound()
        {
            // Act
            var result = await _manager.UpdateCourseAsync(999, new UpdateCourseDto());

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task UpdateCourseAsync_ShouldSucceed_WhenValid()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var course = new Course { Id = 1, Code = "C1", Name = "Old", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept };
            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            var dto = new UpdateCourseDto { Name = "New" };

            // Act
            var result = await _manager.UpdateCourseAsync(1, dto);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            course.Name.Should().Be("New");
        }

        #endregion

        #region DeleteCourseAsync Tests

        [Fact]
        public async Task DeleteCourseAsync_ShouldFail_WhenNotFound()
        {
            // Act
            var result = await _manager.DeleteCourseAsync(999);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task DeleteCourseAsync_ShouldSucceed_WhenValid()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept };
            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.DeleteCourseAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            var deleted = await _context.Courses.FindAsync(1);
            deleted.Should().BeNull();
        }

        #endregion

        #region GetPrerequisitesAsync Tests

        [Fact]
        public async Task GetPrerequisitesAsync_ShouldReturnPrereqs()
        {
            // Arrange - Need to properly seed courses first, then create prerequisite relationship
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var c1 = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept };
            var c2 = new Course { Id = 2, Code = "C2", Name = "C2", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept };
            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddRangeAsync(c1, c2);
            await _context.SaveChangesAsync();

            // Add prerequisite after courses are saved
            var prereq = new CoursePrerequisite { CourseId = 1, PrerequisiteCourseId = 2 };
            await _context.CoursePrerequisites.AddAsync(prereq);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetPrerequisitesAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data.First().CourseCode.Should().Be("C2");
        }

        [Fact]
        public async Task GetAllCoursesAsync_ShouldFilterBySearch()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var c1 = new Course { Id = 1, Code = "CS101", Name = "Introduction to Programming", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept };
            var c2 = new Course { Id = 2, Code = "MTH201", Name = "Calculus", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept };
            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddRangeAsync(c1, c2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetAllCoursesAsync(1, 10, search: "Programming");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data.First().Code.Should().Be("CS101");
        }

        [Fact]
        public async Task UpdateCourseAsync_ShouldUpdateCreditsAndECTS()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var course = new Course { Id = 1, Code = "C1", Name = "Old", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept };
            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            var dto = new UpdateCourseDto { Credits = 4, ECTS = 6 };

            // Act
            var result = await _manager.UpdateCourseAsync(1, dto);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            var updated = await _context.Courses.FindAsync(1);
            updated!.Credits.Should().Be(4);
            updated.ECTS.Should().Be(6);
        }

        [Fact]
        public async Task GetPrerequisitesAsync_ShouldFail_WhenCourseNotFound()
        {
            // Act
            var result = await _manager.GetPrerequisitesAsync(999);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        #endregion
    }
}
