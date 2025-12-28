using System;
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
    public class EfScheduleDalTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EfScheduleDal _dal;

        public EfScheduleDalTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _dal = new EfScheduleDal(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetBySectionIdAsync_ShouldReturnActiveSchedules()
        {
            // Arrange
            var course = new Course { Code = "CS101", Name = "Test Course", Credits = 3, IsActive = true };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var section = new CourseSection 
            { 
                SectionNumber = "A", 
                Semester = "Fall", 
                Year = 2024, 
                Capacity = 30, 
                CourseId = course.Id,
                InstructorId = 1,
                IsActive = true
            };
            _context.CourseSections.Add(section);
            await _context.SaveChangesAsync();

            var classroom = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            _context.Schedules.Add(new Schedule 
            { 
                SectionId = section.Id, 
                ClassroomId = classroom.Id,
                IsActive = true, 
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11)
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetBySectionIdAsync(section.Id);

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_ShouldIncludeRelations()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "CS101", Name = "Test Course", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            _context.Departments.Add(dept);
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection 
            { 
                Id = 1,
                SectionNumber = "A", 
                Semester = "Fall", 
                Year = 2024, 
                Capacity = 30, 
                CourseId = 1,
                Course = course,
                InstructorId = 1,
                Instructor = instructor,
                IsActive = true
            };
            _context.Users.Add(user);
            _context.Faculties.Add(instructor);
            _context.CourseSections.Add(section);
            await _context.SaveChangesAsync();

            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            var schedule = new Schedule 
            { 
                Id = 1,
                SectionId = 1,
                Section = section,
                ClassroomId = 1,
                Classroom = classroom,
                IsActive = true, 
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11)
            };
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByIdWithDetailsAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Section.Should().NotBeNull();
            result.Classroom.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByClassroomIdAsync_ShouldReturnSchedules()
        {
            // Arrange
            var course = new Course { Code = "CS101", Name = "Test Course", Credits = 3, IsActive = true };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var section = new CourseSection 
            { 
                SectionNumber = "A", 
                Semester = "Fall", 
                Year = 2024, 
                Capacity = 30, 
                CourseId = course.Id,
                InstructorId = 1,
                IsActive = true
            };
            _context.CourseSections.Add(section);
            await _context.SaveChangesAsync();

            var classroom = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            _context.Schedules.AddRange(
                new Schedule { SectionId = section.Id, ClassroomId = classroom.Id, IsActive = true, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11) },
                new Schedule { SectionId = section.Id, ClassroomId = classroom.Id, IsActive = true, DayOfWeek = DayOfWeek.Wednesday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11) }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByClassroomIdAsync(classroom.Id);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByClassroomIdAsync_ShouldFilterByDayOfWeek()
        {
            // Arrange
            var course = new Course { Code = "CS101", Name = "Test Course", Credits = 3, IsActive = true };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var section = new CourseSection 
            { 
                SectionNumber = "A", 
                Semester = "Fall", 
                Year = 2024, 
                Capacity = 30, 
                CourseId = course.Id,
                InstructorId = 1,
                IsActive = true
            };
            _context.CourseSections.Add(section);
            await _context.SaveChangesAsync();

            var classroom = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            _context.Schedules.AddRange(
                new Schedule { SectionId = section.Id, ClassroomId = classroom.Id, IsActive = true, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11) },
                new Schedule { SectionId = section.Id, ClassroomId = classroom.Id, IsActive = true, DayOfWeek = DayOfWeek.Wednesday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11) }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByClassroomIdAsync(classroom.Id, DayOfWeek.Monday);

            // Assert
            result.Should().HaveCount(1);
            result.First().DayOfWeek.Should().Be(DayOfWeek.Monday);
        }

        [Fact]
        public async Task GetByInstructorIdAsync_ShouldReturnSchedules()
        {
            // Arrange
            var course = new Course { Code = "CS101", Name = "Test Course", Credits = 3, IsActive = true };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var section = new CourseSection 
            { 
                SectionNumber = "A", 
                Semester = "Fall", 
                Year = 2024, 
                Capacity = 30, 
                CourseId = course.Id,
                InstructorId = 1,
                IsActive = true
            };
            _context.CourseSections.Add(section);
            await _context.SaveChangesAsync();

            var classroom = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            _context.Schedules.Add(new Schedule 
            { 
                SectionId = section.Id, 
                ClassroomId = classroom.Id,
                IsActive = true, 
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11)
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetByInstructorIdAsync(1);

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task HasConflictAsync_ShouldReturnTrue_WhenConflictExists()
        {
            // Arrange
            var course = new Course { Code = "CS101", Name = "Test Course", Credits = 3, IsActive = true };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var section = new CourseSection 
            { 
                SectionNumber = "A", 
                Semester = "Fall", 
                Year = 2024, 
                Capacity = 30, 
                CourseId = course.Id,
                InstructorId = 1,
                IsActive = true
            };
            _context.CourseSections.Add(section);
            await _context.SaveChangesAsync();

            var classroom = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            _context.Schedules.Add(new Schedule 
            { 
                SectionId = section.Id, 
                ClassroomId = classroom.Id,
                IsActive = true, 
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11)
            });
            await _context.SaveChangesAsync();

            // Act - Overlapping time
            var result = await _dal.HasConflictAsync(classroom.Id, DayOfWeek.Monday, TimeSpan.FromHours(10), TimeSpan.FromHours(12));

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasConflictAsync_ShouldReturnFalse_WhenNoConflict()
        {
            // Arrange
            var course = new Course { Code = "CS101", Name = "Test Course", Credits = 3, IsActive = true };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var section = new CourseSection 
            { 
                SectionNumber = "A", 
                Semester = "Fall", 
                Year = 2024, 
                Capacity = 30, 
                CourseId = course.Id,
                InstructorId = 1,
                IsActive = true
            };
            _context.CourseSections.Add(section);
            await _context.SaveChangesAsync();

            var classroom = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            _context.Schedules.Add(new Schedule 
            { 
                SectionId = section.Id, 
                ClassroomId = classroom.Id,
                IsActive = true, 
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11)
            });
            await _context.SaveChangesAsync();

            // Act - Non-overlapping time
            var result = await _dal.HasConflictAsync(classroom.Id, DayOfWeek.Monday, TimeSpan.FromHours(13), TimeSpan.FromHours(15));

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetConflictingScheduleAsync_ShouldReturnSchedule_WhenConflictExists()
        {
            // Arrange
            var course = new Course { Code = "CS101", Name = "Test Course", Credits = 3, IsActive = true };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var section = new CourseSection 
            { 
                SectionNumber = "A", 
                Semester = "Fall", 
                Year = 2024, 
                Capacity = 30, 
                CourseId = course.Id,
                InstructorId = 1,
                IsActive = true
            };
            _context.CourseSections.Add(section);
            await _context.SaveChangesAsync();

            var classroom = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            var existingSchedule = new Schedule 
            { 
                SectionId = section.Id, 
                ClassroomId = classroom.Id,
                IsActive = true, 
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11)
            };
            _context.Schedules.Add(existingSchedule);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetConflictingScheduleAsync(classroom.Id, DayOfWeek.Monday, TimeSpan.FromHours(10), TimeSpan.FromHours(12));

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(existingSchedule.Id);
        }

        [Fact]
        public async Task GetSectionConflictAsync_ShouldReturnSchedule_WhenConflictExists()
        {
            // Arrange
            var course = new Course { Code = "CS101", Name = "Test Course", Credits = 3, IsActive = true };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var section = new CourseSection 
            { 
                SectionNumber = "A", 
                Semester = "Fall", 
                Year = 2024, 
                Capacity = 30, 
                CourseId = course.Id,
                InstructorId = 1,
                IsActive = true
            };
            _context.CourseSections.Add(section);
            await _context.SaveChangesAsync();

            var classroom = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            var existingSchedule = new Schedule 
            { 
                SectionId = section.Id, 
                ClassroomId = classroom.Id,
                IsActive = true, 
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11)
            };
            _context.Schedules.Add(existingSchedule);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetSectionConflictAsync(section.Id, DayOfWeek.Monday, TimeSpan.FromHours(10), TimeSpan.FromHours(12));

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(existingSchedule.Id);
        }

        [Fact]
        public async Task GetSchedulesBySectionIdsAsync_ShouldReturnSchedules()
        {
            // Arrange
            var course = new Course { Code = "CS101", Name = "Test Course", Credits = 3, IsActive = true };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var section1 = new CourseSection { SectionNumber = "A", Semester = "Fall", Year = 2024, Capacity = 30, CourseId = course.Id, InstructorId = 1, IsActive = true };
            var section2 = new CourseSection { SectionNumber = "B", Semester = "Fall", Year = 2024, Capacity = 30, CourseId = course.Id, InstructorId = 1, IsActive = true };
            _context.CourseSections.AddRange(section1, section2);
            await _context.SaveChangesAsync();

            var classroom = new Classroom { Building = "A", RoomNumber = "101", Capacity = 50 };
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            _context.Schedules.AddRange(
                new Schedule { SectionId = section1.Id, ClassroomId = classroom.Id, IsActive = true, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11) },
                new Schedule { SectionId = section2.Id, ClassroomId = classroom.Id, IsActive = true, DayOfWeek = DayOfWeek.Tuesday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11) }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _dal.GetSchedulesBySectionIdsAsync(new List<int> { section1.Id, section2.Id });

            // Assert
            result.Should().HaveCount(2);
        }
    }
}
