using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class ScheduleManagerAutomaticScheduleTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly ScheduleManager _manager;

        public ScheduleManagerAutomaticScheduleTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new CampusContext(options);
            _manager = new ScheduleManager(new UnitOfWork(_context));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GenerateAutomaticScheduleAsync_ShouldReturnFail_WhenNoSections()
        {
            var dto = new AutoScheduleRequestDto { Semester = "Fall", Year = 2024 };

            var result = await _manager.GenerateAutomaticScheduleAsync(dto);

            result.IsSuccessful.Should().BeTrue();
            result.Data!.IsSuccess.Should().BeFalse();
            result.Data.TotalSections.Should().Be(0);
        }

        [Fact]
        public async Task GenerateAutomaticScheduleAsync_ShouldReturnFail_WhenNoClassrooms()
        {
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Users.AddAsync(user);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();

            var dto = new AutoScheduleRequestDto { Semester = "Fall", Year = 2024, MaxIterations = 100 };

            var result = await _manager.GenerateAutomaticScheduleAsync(dto);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task GenerateAutomaticScheduleAsync_ShouldGenerateSchedule_WhenValid()
        {
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Users.AddAsync(user);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.Classrooms.AddAsync(classroom);
            await _context.SaveChangesAsync();

            var dto = new AutoScheduleRequestDto
            {
                Semester = "Fall",
                Year = 2024,
                MaxIterations = 1000
            };

            var result = await _manager.GenerateAutomaticScheduleAsync(dto);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.TotalSections.Should().BeGreaterThan(0);
        }
    }
}

