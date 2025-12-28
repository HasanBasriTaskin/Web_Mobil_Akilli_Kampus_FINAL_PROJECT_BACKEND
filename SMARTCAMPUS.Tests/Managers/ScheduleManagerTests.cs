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
    public class ScheduleManagerTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly ScheduleManager _manager;

        public ScheduleManagerTests()
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
        public async Task GetSchedulesBySectionAsync_ShouldReturnSchedules()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            var schedule = new Schedule { Id = 1, SectionId = 1, Section = section, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.Classrooms.AddAsync(classroom);
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            var result = await _manager.GetSchedulesBySectionAsync(1);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetWeeklyScheduleAsync_ShouldReturnSuccess_WhenSectionExists()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            var schedule = new Schedule { Id = 1, SectionId = 1, Section = section, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.Classrooms.AddAsync(classroom);
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            var result = await _manager.GetWeeklyScheduleAsync(1);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetWeeklyScheduleAsync_ShouldReturnFail_WhenSectionNotFound()
        {
            var result = await _manager.GetWeeklyScheduleAsync(999);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetSchedulesByClassroomAsync_ShouldReturnSchedules()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            var schedule = new Schedule { Id = 1, SectionId = 1, Section = section, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.Classrooms.AddAsync(classroom);
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            var result = await _manager.GetSchedulesByClassroomAsync(1);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetSchedulesByInstructorAsync_ShouldReturnSchedules()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            var schedule = new Schedule { Id = 1, SectionId = 1, Section = section, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.Classrooms.AddAsync(classroom);
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            var result = await _manager.GetSchedulesByInstructorAsync(1);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task CreateScheduleAsync_ShouldReturnSuccess_WhenValid()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.Classrooms.AddAsync(classroom);
            await _context.SaveChangesAsync();

            var dto = new ScheduleCreateDto
            {
                SectionId = 1,
                ClassroomId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(10, 30, 0)
            };

            var result = await _manager.CreateScheduleAsync(dto);

            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task CreateScheduleAsync_ShouldReturnFail_WhenSectionNotFound()
        {
            var dto = new ScheduleCreateDto { SectionId = 999, ClassroomId = 1 };

            var result = await _manager.CreateScheduleAsync(dto);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task CreateScheduleAsync_ShouldReturnFail_WhenClassroomNotFound()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();

            var dto = new ScheduleCreateDto { SectionId = 1, ClassroomId = 999 };

            var result = await _manager.CreateScheduleAsync(dto);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task UpdateScheduleAsync_ShouldReturnSuccess_WhenValid()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            var schedule = new Schedule { Id = 1, SectionId = 1, Section = section, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.Classrooms.AddAsync(classroom);
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            var dto = new ScheduleUpdateDto { DayOfWeek = DayOfWeek.Tuesday };

            var result = await _manager.UpdateScheduleAsync(1, dto);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateScheduleAsync_ShouldReturnFail_WhenNotFound()
        {
            var dto = new ScheduleUpdateDto();

            var result = await _manager.UpdateScheduleAsync(999, dto);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task DeleteScheduleAsync_ShouldReturnSuccess_WhenValid()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            var schedule = new Schedule { Id = 1, SectionId = 1, Section = section, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.Classrooms.AddAsync(classroom);
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            var result = await _manager.DeleteScheduleAsync(1);

            result.IsSuccessful.Should().BeTrue();
            var dbSchedule = await _context.Schedules.FindAsync(1);
            dbSchedule!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteScheduleAsync_ShouldReturnFail_WhenNotFound()
        {
            var result = await _manager.DeleteScheduleAsync(999);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task CheckConflictsAsync_ShouldReturnNoConflicts_WhenNoConflict()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.Classrooms.AddAsync(classroom);
            await _context.SaveChangesAsync();

            var dto = new ScheduleCreateDto
            {
                SectionId = 1,
                ClassroomId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(10, 30, 0)
            };

            var result = await _manager.CheckConflictsAsync(dto);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSchedulesByClassroomAsync_ShouldFilterByDayOfWeek()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            var schedule = new Schedule { Id = 1, SectionId = 1, Section = section, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.Classrooms.AddAsync(classroom);
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            var result = await _manager.GetSchedulesByClassroomAsync(1, DayOfWeek.Monday);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetSchedulesByInstructorAsync_ShouldFilterByDayOfWeek()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            var schedule = new Schedule { Id = 1, SectionId = 1, Section = section, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.Classrooms.AddAsync(classroom);
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            var result = await _manager.GetSchedulesByInstructorAsync(1, DayOfWeek.Monday);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task CreateScheduleAsync_ShouldReturnFail_WhenConflict()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            var section1 = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var section2 = new CourseSection { Id = 2, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "02", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            var existingSchedule = new Schedule { Id = 1, SectionId = 1, Section = section1, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddRangeAsync(section1, section2);
            await _context.Classrooms.AddAsync(classroom);
            await _context.Schedules.AddAsync(existingSchedule);
            await _context.SaveChangesAsync();

            var dto = new ScheduleCreateDto
            {
                SectionId = 2,
                ClassroomId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeSpan(9, 15, 0),
                EndTime = new TimeSpan(10, 45, 0)
            };

            var result = await _manager.CreateScheduleAsync(dto);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task ExportSectionToICalAsync_ShouldReturnSuccess()
        {
            var user = new User { Id = "u1", FullName = "Instructor", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            var schedule = new Schedule { Id = 1, SectionId = 1, Section = section, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.Classrooms.AddAsync(classroom);
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            var result = await _manager.ExportSectionToICalAsync(1);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().Contain("BEGIN:VCALENDAR");
        }

        [Fact]
        public async Task ExportSectionToICalAsync_ShouldReturnFail_WhenSectionNotFound()
        {
            var result = await _manager.ExportSectionToICalAsync(999);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task ExportStudentScheduleToICalAsync_ShouldReturnSuccess()
        {
            var user = new User { Id = "u1", FullName = "Student", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var student = new Student { Id = 1, StudentNumber = "S1", UserId = "u1", User = user, DepartmentId = 1, IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "Course1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u2", User = new User { Id = "u2", FullName = "Instructor", IsActive = true }, EmployeeNumber = "E1", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "01", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            var schedule = new Schedule { Id = 1, SectionId = 1, Section = section, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), IsActive = true };
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Status = EnrollmentStatus.Enrolled, IsActive = true };

            await _context.Users.AddRangeAsync(user, instructor.User);
            await _context.Departments.AddAsync(dept);
            await _context.Students.AddAsync(student);
            await _context.Courses.AddAsync(course);
            await _context.Faculties.AddAsync(instructor);
            await _context.CourseSections.AddAsync(section);
            await _context.Classrooms.AddAsync(classroom);
            await _context.Schedules.AddAsync(schedule);
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            var result = await _manager.ExportStudentScheduleToICalAsync("u1");

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().Contain("BEGIN:VCALENDAR");
        }

        [Fact]
        public async Task ExportStudentScheduleToICalAsync_ShouldReturnFail_WhenStudentNotFound()
        {
            var result = await _manager.ExportStudentScheduleToICalAsync("nonexistent");

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }
    }
}

