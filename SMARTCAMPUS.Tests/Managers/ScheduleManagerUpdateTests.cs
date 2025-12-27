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
    public class ScheduleManagerUpdateTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly ScheduleManager _manager;

        public ScheduleManagerUpdateTests()
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
        public async Task UpdateScheduleAsync_ShouldUpdateAllFields_WhenAllProvided()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            var section1 = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var section2 = new CourseSection { Id = 2, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "02", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom1 = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            var classroom2 = new Classroom { Id = 2, Building = "B", RoomNumber = "201", Capacity = 50, IsActive = true };
            var schedule = new Schedule { Id = 1, SectionId = 1, Section = section1, ClassroomId = 1, Classroom = classroom1, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), IsActive = true };

            await _context.Users.AddAsync(user);
            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddRangeAsync(section1, section2);
            await _context.Classrooms.AddRangeAsync(classroom1, classroom2);
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            var dto = new ScheduleUpdateDto
            {
                SectionId = 2,
                ClassroomId = 2,
                DayOfWeek = DayOfWeek.Tuesday,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(11, 30, 0)
            };

            var result = await _manager.UpdateScheduleAsync(1, dto);

            result.IsSuccessful.Should().BeTrue();
            var updated = await _context.Schedules.FindAsync(1);
            updated!.SectionId.Should().Be(2);
            updated.ClassroomId.Should().Be(2);
            updated.DayOfWeek.Should().Be(DayOfWeek.Tuesday);
        }

        [Fact]
        public async Task UpdateScheduleAsync_ShouldUpdatePartialFields_WhenSomeProvided()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            var schedule = new Schedule { Id = 1, SectionId = 1, Section = section, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), IsActive = true };

            await _context.Users.AddAsync(user);
            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.Classrooms.AddAsync(classroom);
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            var dto = new ScheduleUpdateDto { DayOfWeek = DayOfWeek.Wednesday };

            var result = await _manager.UpdateScheduleAsync(1, dto);

            result.IsSuccessful.Should().BeTrue();
            var updated = await _context.Schedules.FindAsync(1);
            updated!.DayOfWeek.Should().Be(DayOfWeek.Wednesday);
            updated.SectionId.Should().Be(1);
        }

        [Fact]
        public async Task UpdateScheduleAsync_ShouldReturnFail_WhenConflict()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            var section1 = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var section2 = new CourseSection { Id = 2, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "02", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            var schedule1 = new Schedule { Id = 1, SectionId = 1, Section = section1, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), IsActive = true };
            var schedule2 = new Schedule { Id = 2, SectionId = 2, Section = section2, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(12, 30, 0), IsActive = true };

            await _context.Users.AddAsync(user);
            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddRangeAsync(section1, section2);
            await _context.Classrooms.AddAsync(classroom);
            await _context.Schedules.AddRangeAsync(schedule1, schedule2);
            await _context.SaveChangesAsync();

            var dto = new ScheduleUpdateDto
            {
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(11, 30, 0)
            };

            var result = await _manager.UpdateScheduleAsync(1, dto);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }
    }
}

