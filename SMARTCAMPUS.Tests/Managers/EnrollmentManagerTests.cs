using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs.Enrollment;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class EnrollmentManagerTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly EnrollmentManager _manager;

        public EnrollmentManagerTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _manager = new EnrollmentManager(new UnitOfWork(_context));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region EnrollInCourseAsync Tests

        [Fact]
        public async Task EnrollInCourseAsync_ShouldFail_WhenSectionNotFound()
        {
            // Act
            var result = await _manager.EnrollInCourseAsync(1, new CreateEnrollmentDto { SectionId = 1 });

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task EnrollInCourseAsync_ShouldFail_WhenSectionFull()
        {
            // Arrange
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            var section = new CourseSection
            {
                Id = 1,
                EnrolledCount = 10,
                Capacity = 10,
                Course = new Course { Code = "C1", Name = "C1", Credits = 3, ECTS = 5, Department = new Department { Name = "Dept", Code = "DEPT" } },
                SectionNumber = "1",
                Semester = "Fall",
                Year = 2024,
                Instructor = instructor
            };
            await _context.Faculties.AddAsync(instructor);
            await _context.Courses.AddAsync(section.Course);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.EnrollInCourseAsync(1, new CreateEnrollmentDto { SectionId = 1 });

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Section is full");
        }

        [Fact]
        public async Task EnrollInCourseAsync_ShouldFail_WhenPrerequisitesNotMet()
        {
            // Arrange
            var dept = new Department { Name = "Dept", Code = "DEPT" };
            var course1 = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, Department = dept };
            var course2 = new Course { Id = 2, Code = "C2", Name = "C2", Credits = 3, ECTS = 5, Department = dept };
            
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            var section = new CourseSection
            {
                Id = 1,
                CourseId = 1,
                EnrolledCount = 0,
                Capacity = 10,
                Course = course1,
                SectionNumber = "1",
                Semester = "Fall",
                Year = 2024,
                Instructor = instructor
            };
            await _context.Faculties.AddAsync(instructor);

            await _context.Courses.AddRangeAsync(course1, course2);
            await _context.CourseSections.AddAsync(section);
            
            // Define Prerequisite: C1 needs C2
            var prereq = new CoursePrerequisite { CourseId = 1, PrerequisiteCourseId = 2, PrerequisiteCourse = course2 };
            await _context.CoursePrerequisites.AddAsync(prereq);
            
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.EnrollInCourseAsync(1, new CreateEnrollmentDto { SectionId = 1 });

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().ContainMatch("*Missing prerequisites*");
        }

        [Fact]
        public async Task EnrollInCourseAsync_ShouldFail_WhenScheduleConflict()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "Dept", Code = "DEPT", IsActive = true };
            await _context.Departments.AddAsync(dept);
            await _context.SaveChangesAsync();
            
            var course1 = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var course2 = new Course { Id = 2, Code = "C2", Name = "C2", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };

            var user = new User { Id = "u1", FullName = "I", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "EMP001", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            var section1 = new CourseSection { Id = 1, CourseId = 1, Course = course1, InstructorId = 1, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var section2 = new CourseSection { Id = 2, CourseId = 2, Course = course2, InstructorId = 1, Instructor = instructor, SectionNumber = "2", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            
            var stdUser = new User { Id = "s1", FullName = "Student", IsActive = true };
            var student = new Student { Id = 1, UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = 1, Department = dept, IsActive = true };

            // Schedules: Same time Monday 10-12
            var classroom = new Classroom { Id = 1, Building = "A", RoomNumber = "101", Capacity = 50, IsActive = true };
            var sched1 = new Schedule { Id = 1, SectionId = 1, Section = section1, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(12, 0, 0), IsActive = true };
            var sched2 = new Schedule { Id = 2, SectionId = 2, Section = section2, ClassroomId = 1, Classroom = classroom, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(12, 0, 0), IsActive = true };
            await _context.Classrooms.AddAsync(classroom);

            // Enroll student in section 1
            var existingEnrollment = new Enrollment { Id = 1, StudentId = 1, Student = student, SectionId = 1, Section = section1, Status = EnrollmentStatus.Enrolled, IsActive = true };

            await _context.Users.AddRangeAsync(user, stdUser);
            await _context.Faculties.AddAsync(instructor);
            await _context.Courses.AddRangeAsync(course1, course2);
            await _context.CourseSections.AddRangeAsync(section1, section2);
            await _context.Students.AddAsync(student);
            await _context.Schedules.AddRangeAsync(sched1, sched2);
            await _context.Enrollments.AddAsync(existingEnrollment);
            await _context.SaveChangesAsync();

            // Act: Try enrolling in section 2
            var result = await _manager.EnrollInCourseAsync(1, new CreateEnrollmentDto { SectionId = 2 });

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().ContainMatch("*Schedule conflict*");
        }

        [Fact]
        public async Task EnrollInCourseAsync_ShouldFail_WhenEnrolledInAnotherSectionOfSameCourse()
        {
             // Arrange
            var dept = new Department { Name = "CS", Code = "CS" };

            var course = new Course { Code = "C1", Name = "C1", Credits = 3, ECTS = 5, Department = dept };
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            
            var section1 = new CourseSection { Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024, Instructor = instructor, Capacity = 30 };
            var section2 = new CourseSection { Course = course, SectionNumber = "2", Semester = "Fall", Year = 2024, Instructor = instructor, Capacity = 30 };
            
            await _context.Departments.AddAsync(dept);
            await _context.Faculties.AddAsync(instructor);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddRangeAsync(section1, section2);

            var stdUser = new User { Id = "s1", FullName = "Student" };
            var student = new Student { UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = dept.Id };
            await _context.Students.AddAsync(student);

            // Enrolled in Section 1 (Using explicit Id=1 if necessary, or let EF generate and we fetch)
            // Ideally we need to know the IDs for the call.
            // EF InMemory gen adds 1, 2...
            // section1 -> Id=1 (probably)
            // section2 -> Id=2
            // student -> Id=1
            await _context.SaveChangesAsync();

            // We need IDs. Fetch them or assume 1 if isolated DB.
            // Using different DB name per test (Guid) ensures clean state.
            // So Section1.Id should be 1.
            
            var existingEnrollment = new Enrollment { StudentId = student.Id, SectionId = section1.Id, Status = EnrollmentStatus.Enrolled, Section = section1, Student = student };
            await _context.Enrollments.AddAsync(existingEnrollment);
            await _context.SaveChangesAsync();

            // Act: Try enrolling in Section 2 (same course)
            var result = await _manager.EnrollInCourseAsync(student.Id, new CreateEnrollmentDto { SectionId = section2.Id });

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Bu dersin başka bir seksiyonuna zaten kayıtlısınız");
        }

        [Fact]
        public async Task EnrollInCourseAsync_ShouldReactivate_WhenAlreadyExistsAndNotPendingOrEnrolled()
        {
             // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            await _context.Departments.AddAsync(dept);
            await _context.SaveChangesAsync();
            
            var user = new User { Id = "u1", FullName = "Prof", IsActive = true };
            var instructor = new Faculty { Id = 1, Title = "Dr.", UserId = "u1", User = user, EmployeeNumber = "E1", DepartmentId = 1, Department = dept, IsActive = true };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };

            var stdUser = new User { Id = "s1", FullName = "Student", IsActive = true };
            var student = new Student { Id = 1, UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = 1, Department = dept, IsActive = true };
            var existingEnrollment = new Enrollment 
            { 
                Id = 1,
                StudentId = 1, 
                SectionId = 1, 
                Status = EnrollmentStatus.Dropped, // Previously dropped
                EnrollmentDate = DateTime.UtcNow.AddDays(-10),
                IsActive = true
            };

            await _context.Users.AddRangeAsync(user, stdUser);
            await _context.Faculties.AddAsync(instructor);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.Students.AddAsync(student);
            await _context.Enrollments.AddAsync(existingEnrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.EnrollInCourseAsync(1, new CreateEnrollmentDto { SectionId = 1 });

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            
            var dbEnrollment = await _context.Enrollments.FindAsync(existingEnrollment.Id);
            dbEnrollment!.Status.Should().Be(EnrollmentStatus.Pending);
        }

        [Fact]
        public async Task EnrollInCourseAsync_ShouldFail_WhenAlreadyEnrolledOrPendingSameSection()
        {
             // Arrange
            var dept = new Department { Name = "CS", Code = "CS" };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, Department = dept };
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024, Instructor = instructor, Capacity = 30 };
            await _context.Departments.AddAsync(dept);
            await _context.Faculties.AddAsync(instructor);
            var stdUser = new User { Id = "s1", FullName = "Student" };
            var student = new Student { Id = 1, UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = dept.Id };
            await _context.Students.AddAsync(student);
            var existingEnrollment = new Enrollment { StudentId = 1, SectionId = 1, Status = EnrollmentStatus.Pending };

            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.Enrollments.AddAsync(existingEnrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.EnrollInCourseAsync(1, new CreateEnrollmentDto { SectionId = 1 });

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Bu derse zaten kayıtlısınız veya onay bekliyor");
        }

        [Fact]
        public async Task EnrollInCourseAsync_ShouldSucceed_WhenValid()
        {
            // Arrange
            var user = new User { FullName = "Prof" };
            var instructor = new Faculty { Title = "Dr.", User = user, EmployeeNumber = "E1" };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, Department = new Department { Name = "Dept", Code = "DEPT" } };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, Instructor = instructor, Capacity = 10, EnrolledCount = 0, SectionNumber = "1", Semester = "Fall", Year = 2024 };

            await _context.Users.AddAsync(user);
            await _context.Faculties.AddAsync(instructor);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.EnrollInCourseAsync(1, new CreateEnrollmentDto { SectionId = 1 });

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
            
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.StudentId == 1 && e.SectionId == 1);
            enrollment.Should().NotBeNull();
            enrollment!.Status.Should().Be(EnrollmentStatus.Pending);
        }

        #endregion

        #region DropCourseAsync Tests

        [Fact]
        public async Task DropCourseAsync_ShouldFail_WhenNotFound()
        {
            // Act
            var result = await _manager.DropCourseAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task DropCourseAsync_ShouldSucceed_WhenFound()
        {
            // Arrange
            var dept = new Department { Name = "CS", Code = "CS" };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, Department = dept };
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, EnrolledCount = 1, SectionNumber = "1", Semester = "Fall", Year = 2024, Instructor = instructor, Capacity = 30 };
            await _context.Departments.AddAsync(dept);
            await _context.Faculties.AddAsync(instructor);
            var stdUser = new User { Id = "s1", FullName = "Student" };
            var student = new Student { Id = 1, UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = dept.Id };
            await _context.Students.AddAsync(student);
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Section = section, EnrollmentDate = DateTime.UtcNow, Status = EnrollmentStatus.Enrolled };
            
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.DropCourseAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            enrollment.Status.Should().Be(EnrollmentStatus.Dropped);
        }

        [Fact]
        public async Task DropCourseAsync_ShouldWithdraw_WhenAfter4Weeks()
        {
            // Arrange
            var dept = new Department { Name = "CS", Code = "CS" };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, Department = dept };
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, SectionNumber = "1", Semester = "Fall", Year = 2024, Instructor = instructor };
            await _context.Departments.AddAsync(dept);
            await _context.Faculties.AddAsync(instructor);
            var stdUser = new User { Id = "s1", FullName = "Student" };
            var student = new Student { Id = 1, UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = dept.Id };
            await _context.Students.AddAsync(student);
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Section = section, EnrollmentDate = DateTime.UtcNow.AddDays(-30), Status = EnrollmentStatus.Enrolled }; // > 4 weeks
            
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.DropCourseAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            enrollment.Status.Should().Be(EnrollmentStatus.Withdrawn);
        }

        #endregion

        #region ApproveEnrollmentAsync Tests

        [Fact]
        public async Task ApproveEnrollmentAsync_ShouldFail_WhenNotFound()
        {
             // Act
            var result = await _manager.ApproveEnrollmentAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task ApproveEnrollmentAsync_ShouldFail_WhenAccessDenied()
        {
            // Arrange - Instructor1 is in dept1, but course/section is in dept2
            var dept1 = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var dept2 = new Department { Id = 2, Name = "EE", Code = "EE", IsActive = true };
            await _context.Departments.AddRangeAsync(dept1, dept2);
            await _context.SaveChangesAsync();
            
            var user1 = new User { Id = "u1", FullName = "I", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user1, EmployeeNumber = "EMP001", Title = "Prof", DepartmentId = 1, Department = dept1, IsActive = true }; // In dept1
            var user2 = new User { Id = "u2", FullName = "I2", IsActive = true };
            var instructor2 = new Faculty { Id = 2, UserId = "u2", User = user2, EmployeeNumber = "EMP002", Title = "Prof", DepartmentId = 2, Department = dept2, IsActive = true }; // In dept2
            await _context.Users.AddRangeAsync(user1, user2);
            await _context.Faculties.AddRangeAsync(instructor, instructor2);
            await _context.SaveChangesAsync();
            
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = 2, Department = dept2, IsActive = true }; // Course in dept2
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
            
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 2, Instructor = instructor2, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var stdUser = new User { Id = "s1", FullName = "Student", IsActive = true };
            var student = new Student { Id = 1, UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = 2, Department = dept2, IsActive = true };
            await _context.Users.AddAsync(stdUser);
            await _context.Students.AddAsync(student);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();
            
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Pending, IsActive = true };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act - Instructor 1 (dept1) tries to approve enrollment for course in dept2
            var result = await _manager.ApproveEnrollmentAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task ApproveEnrollmentAsync_ShouldFail_WhenNotPending()
        {
            // Arrange
            var dept = new Department { Name = "CS", Code = "CS" };
            await _context.Departments.AddAsync(dept);
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            await _context.Faculties.AddAsync(instructor);
            await _context.SaveChangesAsync();
            
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = dept.Id, Department = dept };
            var section = new CourseSection { CourseId = 1, Course = course, InstructorId = instructor.Id, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30 };
            await _context.Courses.AddAsync(course);
            var stdUser = new User { Id = "s1", FullName = "Student" };
            var student = new Student { UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = dept.Id };
            await _context.Students.AddAsync(student);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();
            
            var enrollment = new Enrollment { StudentId = student.Id, SectionId = section.Id, Section = section, Status = EnrollmentStatus.Enrolled };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.ApproveEnrollmentAsync(enrollment.Id, instructor.Id);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task ApproveEnrollmentAsync_ShouldFail_WhenSectionFull()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            await _context.Departments.AddAsync(dept);
            await _context.SaveChangesAsync();
            
            var user = new User { Id = "u1", FullName = "I", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "EMP001", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            await _context.Faculties.AddAsync(instructor);
            await _context.SaveChangesAsync();
            
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
            
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, Capacity = 10, EnrolledCount = 10, SectionNumber = "1", Semester = "Fall", Year = 2024, IsActive = true };
            var stdUser = new User { Id = "s1", FullName = "Student", IsActive = true };
            var student = new Student { Id = 1, UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(stdUser);
            await _context.Students.AddAsync(student);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();
            
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Pending, IsActive = true };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.ApproveEnrollmentAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Section is now full, cannot approve");
        }

        [Fact]
        public async Task ApproveEnrollmentAsync_ShouldSucceed()
        {
            // Arrange
            var dept = new Department { Name = "CS", Code = "CS" };
            await _context.Departments.AddAsync(dept);
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof", DepartmentId = dept.Id, Department = dept };
            await _context.Faculties.AddAsync(instructor);
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = dept.Id, Department = dept };
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
            
            var section = new CourseSection { CourseId = 1, Course = course, InstructorId = instructor.Id, Capacity = 10, EnrolledCount = 0, SectionNumber = "1", Semester = "Fall", Year = 2024, Instructor = instructor };
            var stdUser = new User { Id = "s1", FullName = "Student" };
            var student = new Student { UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = dept.Id };
            await _context.Students.AddAsync(student);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();
            
            var enrollment = new Enrollment { StudentId = student.Id, SectionId = section.Id, Section = section, Status = EnrollmentStatus.Pending };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.ApproveEnrollmentAsync(enrollment.Id, instructor.Id);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            var dbEnrollment = await _context.Enrollments.FindAsync(enrollment.Id);
            dbEnrollment!.Status.Should().Be(EnrollmentStatus.Enrolled);
        }

        #endregion

        #region RejectEnrollmentAsync Tests

        [Fact]
        public async Task RejectEnrollmentAsync_ShouldFail_WhenNotFound()
        {
             // Act
            var result = await _manager.RejectEnrollmentAsync(1, 1, null);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task RejectEnrollmentAsync_ShouldFail_WhenAccessDenied()
        {
            // Arrange - Instructor is in dept1, but course/section is in dept2
            var dept1 = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var dept2 = new Department { Id = 2, Name = "EE", Code = "EE", IsActive = true };
            await _context.Departments.AddRangeAsync(dept1, dept2);
            await _context.SaveChangesAsync();
            
            var user1 = new User { Id = "u1", FullName = "I", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user1, EmployeeNumber = "EMP001", Title = "Prof", DepartmentId = 1, Department = dept1, IsActive = true }; // In dept1
            var user2 = new User { Id = "u2", FullName = "I2", IsActive = true };
            var instructor2 = new Faculty { Id = 2, UserId = "u2", User = user2, EmployeeNumber = "EMP002", Title = "Prof", DepartmentId = 2, Department = dept2, IsActive = true }; // In dept2
            await _context.Users.AddRangeAsync(user1, user2);
            await _context.Faculties.AddRangeAsync(instructor, instructor2);
            await _context.SaveChangesAsync();
            
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = 2, Department = dept2, IsActive = true }; // Course in dept2
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
            
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 2, Instructor = instructor2, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var stdUser = new User { Id = "s1", FullName = "Student", IsActive = true };
            var student = new Student { Id = 1, UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = 2, Department = dept2, IsActive = true };
            await _context.Users.AddAsync(stdUser);
            await _context.Students.AddAsync(student);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();
            
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Pending, IsActive = true };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act - Instructor 1 (dept1) tries to reject enrollment for course in dept2
            var result = await _manager.RejectEnrollmentAsync(1, 1, null);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task RejectEnrollmentAsync_ShouldFail_WhenNotPending()
        {
             // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            await _context.Departments.AddAsync(dept);
            await _context.SaveChangesAsync();
            
            var user = new User { Id = "u1", FullName = "I", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "EMP001", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            await _context.Faculties.AddAsync(instructor);
            await _context.SaveChangesAsync();
            
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
            
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var stdUser = new User { Id = "s1", FullName = "Student", IsActive = true };
            var student = new Student { Id = 1, UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(stdUser);
            await _context.Students.AddAsync(student);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();
            
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Enrolled, IsActive = true };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.RejectEnrollmentAsync(1, 1, null);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task RejectEnrollmentAsync_ShouldSucceed()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            await _context.Departments.AddAsync(dept);
            await _context.SaveChangesAsync();
            
            var user = new User { Id = "u1", FullName = "I", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = user, EmployeeNumber = "EMP001", Title = "Prof", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(user);
            await _context.Faculties.AddAsync(instructor);
            
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
            
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var stdUser = new User { Id = "s1", FullName = "Student", IsActive = true };
            var student = new Student { Id = 1, UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = 1, Department = dept, IsActive = true };
            await _context.Users.AddAsync(stdUser);
            await _context.Students.AddAsync(student);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();
            
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Pending, IsActive = true };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.RejectEnrollmentAsync(1, 1, "Full");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            var dbEnrollment = await _context.Enrollments.FindAsync(1);
            dbEnrollment!.Status.Should().Be(EnrollmentStatus.Rejected);
        }

        #endregion

        #region GetPendingEnrollmentsAsync Tests

        [Fact]
        public async Task GetPendingEnrollmentsAsync_ShouldFail_WhenSectionNotFound()
        {
            // Act
            var result = await _manager.GetPendingEnrollmentsAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetPendingEnrollmentsAsync_ShouldReturnPending()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "Student" };
            var student = new Student { Id = 1, UserId = "u1", User = user, StudentNumber = "S1" };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, DepartmentId = dept.Id, Department = dept };
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, SectionNumber = "1", Semester = "Fall", Year = 2024, Instructor = instructor, Capacity = 30 };
            await _context.Departments.AddAsync(dept);
            await _context.Faculties.AddAsync(instructor);
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Status = EnrollmentStatus.Pending, Student = student, Section = section };

            await _context.Users.AddAsync(user);
            await _context.Students.AddAsync(student);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetPendingEnrollmentsAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        #endregion

        #region GetMyCoursesAsync Tests

        [Fact]
        public async Task GetMyCoursesAsync_ShouldReturnEnrolledCourses()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            await _context.Departments.AddAsync(dept);
            await _context.SaveChangesAsync();

            var instructorUser = new User { Id = "u1", FullName = "Prof. Instructor", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = instructorUser, EmployeeNumber = "EMP001", Title = "Prof.", DepartmentId = 1, Department = dept, IsActive = true };
            
            var course1 = new Course { Id = 1, Code = "CS101", Name = "Introduction to CS", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var course2 = new Course { Id = 2, Code = "CS102", Name = "Data Structures", Credits = 4, ECTS = 6, DepartmentId = 1, Department = dept, IsActive = true };
            
            var section1 = new CourseSection { Id = 1, CourseId = 1, Course = course1, InstructorId = 1, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };
            var section2 = new CourseSection { Id = 2, CourseId = 2, Course = course2, InstructorId = 1, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };

            var studentUser = new User { Id = "s1", FullName = "Student One", IsActive = true };
            var student = new Student { Id = 1, UserId = "s1", User = studentUser, StudentNumber = "12345", DepartmentId = 1, Department = dept, IsActive = true };

            var enrollment1 = new Enrollment { Id = 1, StudentId = 1, Student = student, SectionId = 1, Section = section1, Status = EnrollmentStatus.Enrolled, EnrollmentDate = DateTime.UtcNow, MidtermGrade = 85, FinalGrade = 90, LetterGrade = "A", IsActive = true };
            var enrollment2 = new Enrollment { Id = 2, StudentId = 1, Student = student, SectionId = 2, Section = section2, Status = EnrollmentStatus.Enrolled, EnrollmentDate = DateTime.UtcNow, IsActive = true };
            var enrollment3 = new Enrollment { Id = 3, StudentId = 1, Student = student, SectionId = 1, Section = section1, Status = EnrollmentStatus.Dropped, EnrollmentDate = DateTime.UtcNow, IsActive = true }; // Should not be included

            await _context.Users.AddRangeAsync(instructorUser, studentUser);
            await _context.Faculties.AddAsync(instructor);
            await _context.Courses.AddRangeAsync(course1, course2);
            await _context.CourseSections.AddRangeAsync(section1, section2);
            await _context.Students.AddAsync(student);
            await _context.Enrollments.AddRangeAsync(enrollment1, enrollment2, enrollment3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetMyCoursesAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.Should().Contain(c => c.CourseCode == "CS101" && c.MidtermGrade == 85 && c.FinalGrade == 90 && c.LetterGrade == "A");
            result.Data.Should().Contain(c => c.CourseCode == "CS102");
            result.Data.Should().NotContain(c => c.Status == EnrollmentStatus.Dropped);
        }

        [Fact]
        public async Task GetMyCoursesAsync_ShouldReturnEmpty_WhenNoEnrollments()
        {
            // Arrange
            var studentUser = new User { Id = "s1", FullName = "Student One", IsActive = true };
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            var student = new Student { Id = 1, UserId = "s1", User = studentUser, StudentNumber = "12345", DepartmentId = 1, Department = dept, IsActive = true };

            await _context.Departments.AddAsync(dept);
            await _context.Users.AddAsync(studentUser);
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetMyCoursesAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region GetStudentsBySectionAsync Tests

        [Fact]
        public async Task GetStudentsBySectionAsync_ShouldReturnEnrolledStudents()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            await _context.Departments.AddAsync(dept);
            await _context.SaveChangesAsync();

            var instructorUser = new User { Id = "u1", FullName = "Prof. Instructor", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = instructorUser, EmployeeNumber = "EMP001", Title = "Prof.", DepartmentId = 1, Department = dept, IsActive = true };
            
            var course = new Course { Id = 1, Code = "CS101", Name = "Introduction to CS", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };

            var student1User = new User { Id = "s1", FullName = "Student One", Email = "student1@test.com", IsActive = true };
            var student1 = new Student { Id = 1, UserId = "s1", User = student1User, StudentNumber = "12345", DepartmentId = 1, Department = dept, IsActive = true };
            
            var student2User = new User { Id = "s2", FullName = "Student Two", Email = "student2@test.com", IsActive = true };
            var student2 = new Student { Id = 2, UserId = "s2", User = student2User, StudentNumber = "12346", DepartmentId = 1, Department = dept, IsActive = true };

            var enrollment1 = new Enrollment { Id = 1, StudentId = 1, Student = student1, SectionId = 1, Section = section, Status = EnrollmentStatus.Enrolled, EnrollmentDate = DateTime.UtcNow, MidtermGrade = 85, FinalGrade = 90, LetterGrade = "A", IsActive = true };
            var enrollment2 = new Enrollment { Id = 2, StudentId = 2, Student = student2, SectionId = 1, Section = section, Status = EnrollmentStatus.Enrolled, EnrollmentDate = DateTime.UtcNow, IsActive = true };
            var enrollment3 = new Enrollment { Id = 3, StudentId = 1, Student = student1, SectionId = 1, Section = section, Status = EnrollmentStatus.Dropped, EnrollmentDate = DateTime.UtcNow, IsActive = true }; // Should not be included

            await _context.Users.AddRangeAsync(instructorUser, student1User, student2User);
            await _context.Faculties.AddAsync(instructor);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.Students.AddRangeAsync(student1, student2);
            await _context.Enrollments.AddRangeAsync(enrollment1, enrollment2, enrollment3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetStudentsBySectionAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.Should().Contain(s => s.StudentNumber == "12345" && s.MidtermGrade == 85 && s.FinalGrade == 90 && s.LetterGrade == "A");
            result.Data.Should().Contain(s => s.StudentNumber == "12346");
            result.Data.Should().NotContain(s => s.Status == EnrollmentStatus.Dropped);
        }

        [Fact]
        public async Task GetStudentsBySectionAsync_ShouldFail_WhenSectionNotFound()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            await _context.Departments.AddAsync(dept);
            await _context.SaveChangesAsync();

            var instructorUser = new User { Id = "u1", FullName = "Prof. Instructor", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = instructorUser, EmployeeNumber = "EMP001", Title = "Prof.", DepartmentId = 1, Department = dept, IsActive = true };

            await _context.Users.AddAsync(instructorUser);
            await _context.Faculties.AddAsync(instructor);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetStudentsBySectionAsync(999, 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.Errors.Should().Contain("Section not found or access denied");
        }

        [Fact]
        public async Task GetStudentsBySectionAsync_ShouldFail_WhenAccessDenied()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            await _context.Departments.AddAsync(dept);
            await _context.SaveChangesAsync();

            var instructor1User = new User { Id = "u1", FullName = "Prof. Instructor 1", IsActive = true };
            var instructor1 = new Faculty { Id = 1, UserId = "u1", User = instructor1User, EmployeeNumber = "EMP001", Title = "Prof.", DepartmentId = 1, Department = dept, IsActive = true };
            
            var instructor2User = new User { Id = "u2", FullName = "Prof. Instructor 2", IsActive = true };
            var instructor2 = new Faculty { Id = 2, UserId = "u2", User = instructor2User, EmployeeNumber = "EMP002", Title = "Prof.", DepartmentId = 1, Department = dept, IsActive = true };
            
            var course = new Course { Id = 1, Code = "CS101", Name = "Introduction to CS", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 2, Instructor = instructor2, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };

            await _context.Users.AddRangeAsync(instructor1User, instructor2User);
            await _context.Faculties.AddRangeAsync(instructor1, instructor2);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();

            // Act - Instructor 1 trying to access Instructor 2's section
            var result = await _manager.GetStudentsBySectionAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.Errors.Should().Contain("Section not found or access denied");
        }

        [Fact]
        public async Task GetStudentsBySectionAsync_ShouldReturnEmpty_WhenNoEnrolledStudents()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            await _context.Departments.AddAsync(dept);
            await _context.SaveChangesAsync();

            var instructorUser = new User { Id = "u1", FullName = "Prof. Instructor", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = instructorUser, EmployeeNumber = "EMP001", Title = "Prof.", DepartmentId = 1, Department = dept, IsActive = true };
            
            var course = new Course { Id = 1, Code = "CS101", Name = "Introduction to CS", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, IsActive = true };

            await _context.Users.AddAsync(instructorUser);
            await _context.Faculties.AddAsync(instructor);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetStudentsBySectionAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region GetMySectionsAsync Tests

        [Fact]
        public async Task GetMySectionsAsync_ShouldReturnInstructorSections()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            await _context.Departments.AddAsync(dept);
            await _context.SaveChangesAsync();

            var instructorUser = new User { Id = "u1", FullName = "Prof. Instructor", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = instructorUser, EmployeeNumber = "EMP001", Title = "Prof.", DepartmentId = 1, Department = dept, IsActive = true };
            
            var course1 = new Course { Id = 1, Code = "CS101", Name = "Introduction to CS", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var course2 = new Course { Id = 2, Code = "CS102", Name = "Data Structures", Credits = 4, ECTS = 6, DepartmentId = 1, Department = dept, IsActive = true };
            
            var section1 = new CourseSection { Id = 1, CourseId = 1, Course = course1, InstructorId = 1, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, EnrolledCount = 10, IsActive = true };
            var section2 = new CourseSection { Id = 2, CourseId = 2, Course = course2, InstructorId = 1, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 25, EnrolledCount = 5, IsActive = true };

            var studentUser = new User { Id = "s1", FullName = "Student One", IsActive = true };
            var student = new Student { Id = 1, UserId = "s1", User = studentUser, StudentNumber = "12345", DepartmentId = 1, Department = dept, IsActive = true };

            // Pending enrollments for section1
            var pendingEnrollment1 = new Enrollment { Id = 1, StudentId = 1, Student = student, SectionId = 1, Section = section1, Status = EnrollmentStatus.Pending, IsActive = true };
            var pendingEnrollment2 = new Enrollment { Id = 2, StudentId = 1, Student = student, SectionId = 1, Section = section1, Status = EnrollmentStatus.Pending, IsActive = true };

            await _context.Users.AddRangeAsync(instructorUser, studentUser);
            await _context.Faculties.AddAsync(instructor);
            await _context.Courses.AddRangeAsync(course1, course2);
            await _context.CourseSections.AddRangeAsync(section1, section2);
            await _context.Students.AddAsync(student);
            await _context.Enrollments.AddRangeAsync(pendingEnrollment1, pendingEnrollment2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetMySectionsAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.Should().Contain(s => s.CourseCode == "CS101" && s.EnrolledCount == 10 && s.PendingCount == 2);
            result.Data.Should().Contain(s => s.CourseCode == "CS102" && s.EnrolledCount == 5 && s.PendingCount == 0);
        }

        [Fact]
        public async Task GetMySectionsAsync_ShouldReturnEmpty_WhenNoSections()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            await _context.Departments.AddAsync(dept);
            await _context.SaveChangesAsync();

            var instructorUser = new User { Id = "u1", FullName = "Prof. Instructor", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = instructorUser, EmployeeNumber = "EMP001", Title = "Prof.", DepartmentId = 1, Department = dept, IsActive = true };

            await _context.Users.AddAsync(instructorUser);
            await _context.Faculties.AddAsync(instructor);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetMySectionsAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMySectionsAsync_ShouldCalculatePendingCountCorrectly()
        {
            // Arrange
            var dept = new Department { Id = 1, Name = "CS", Code = "CS", IsActive = true };
            await _context.Departments.AddAsync(dept);
            await _context.SaveChangesAsync();

            var instructorUser = new User { Id = "u1", FullName = "Prof. Instructor", IsActive = true };
            var instructor = new Faculty { Id = 1, UserId = "u1", User = instructorUser, EmployeeNumber = "EMP001", Title = "Prof.", DepartmentId = 1, Department = dept, IsActive = true };
            
            var course = new Course { Id = 1, Code = "CS101", Name = "Introduction to CS", Credits = 3, ECTS = 5, DepartmentId = 1, Department = dept, IsActive = true };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, InstructorId = 1, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30, EnrolledCount = 5, IsActive = true };

            var student1User = new User { Id = "s1", FullName = "Student One", IsActive = true };
            var student1 = new Student { Id = 1, UserId = "s1", User = student1User, StudentNumber = "12345", DepartmentId = 1, Department = dept, IsActive = true };
            
            var student2User = new User { Id = "s2", FullName = "Student Two", IsActive = true };
            var student2 = new Student { Id = 2, UserId = "s2", User = student2User, StudentNumber = "12346", DepartmentId = 1, Department = dept, IsActive = true };

            // 3 pending enrollments
            var pending1 = new Enrollment { Id = 1, StudentId = 1, Student = student1, SectionId = 1, Section = section, Status = EnrollmentStatus.Pending, IsActive = true };
            var pending2 = new Enrollment { Id = 2, StudentId = 2, Student = student2, SectionId = 1, Section = section, Status = EnrollmentStatus.Pending, IsActive = true };
            var pending3 = new Enrollment { Id = 3, StudentId = 1, Student = student1, SectionId = 1, Section = section, Status = EnrollmentStatus.Pending, IsActive = true };
            
            // 1 enrolled (should not count as pending)
            var enrolled = new Enrollment { Id = 4, StudentId = 1, Student = student1, SectionId = 1, Section = section, Status = EnrollmentStatus.Enrolled, IsActive = true };

            await _context.Users.AddRangeAsync(instructorUser, student1User, student2User);
            await _context.Faculties.AddAsync(instructor);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.Students.AddRangeAsync(student1, student2);
            await _context.Enrollments.AddRangeAsync(pending1, pending2, pending3, enrolled);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetMySectionsAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data.First().PendingCount.Should().Be(3);
        }

        #endregion
        
        // Removed tests for CheckPrerequisitesAsync and CheckScheduleConflictAsync as they are covered via EnrollInCourseAsync integration scenarios
        // or effectively tested implicitly. Added specific scenarios in EnrollInCourseAsync above.
    }
}
