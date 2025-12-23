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
            var dept = new Department { Name = "Dept", Code = "DEPT" };
            var course1 = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, Department = dept };
            var course2 = new Course { Id = 2, Code = "C2", Name = "C2", Credits = 3, ECTS = 5, Department = dept };

            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            var section1 = new CourseSection { Id = 1, CourseId = 1, Course = course1, SectionNumber = "1", Semester = "Fall", Year = 2024, Instructor = instructor, Capacity = 30 };
            var section2 = new CourseSection { Id = 2, CourseId = 2, Course = course2, SectionNumber = "2", Semester = "Fall", Year = 2024, Instructor = instructor, Capacity = 30 };
            await _context.Faculties.AddAsync(instructor);

            // Schedules: Same time Monday 10-12
            var sched1 = new Schedule { SectionId = 1, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(12, 0, 0) };
            var sched2 = new Schedule { SectionId = 2, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(12, 0, 0) };

            // Enroll student in section 1
            var existingEnrollment = new Enrollment { StudentId = 1, SectionId = 1, Status = EnrollmentStatus.Enrolled };

            await _context.Courses.AddRangeAsync(course1, course2);
            await _context.CourseSections.AddRangeAsync(section1, section2);
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
            var user = new User { FullName = "Prof" };
            var instructor = new Faculty { Title = "Dr.", User = user, EmployeeNumber = "E1" };
            var course = new Course { Id = 1, Code = "C1", Name = "C1", Credits = 3, ECTS = 5, Department = new Department { Name = "Dept", Code = "DEPT" } };
            var section = new CourseSection { Id = 1, CourseId = 1, Course = course, Instructor = instructor, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30 };

            var dept = new Department { Id = 1, Name = "CS", Code = "CS" };
            var stdUser = new User { Id = "s1", FullName = "Student" };
            var student = new Student { Id = 1, UserId = "s1", User = stdUser, StudentNumber = "123", Department = dept };
            await _context.Departments.AddAsync(dept);
            await _context.Students.AddAsync(student);
            var existingEnrollment = new Enrollment 
            { 
                StudentId = 1, 
                SectionId = 1, 
                Status = EnrollmentStatus.Dropped, // Previously dropped
                EnrollmentDate = DateTime.UtcNow.AddDays(-10) 
            };

            await _context.Users.AddAsync(user);
            await _context.Faculties.AddAsync(instructor);
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
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
            // Arrange
            var dept = new Department { Name = "CS", Code = "CS" };
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            await _context.Departments.AddAsync(dept);
            await _context.Faculties.AddAsync(instructor);
            await _context.SaveChangesAsync();
            
            var section = new CourseSection { InstructorId = instructor.Id + 1, SectionNumber = "1", Semester = "Fall", Year = 2024, Instructor = instructor, Capacity = 30 }; // Different Instructor
            var stdUser = new User { Id = "s1", FullName = "Student" };
            var student = new Student { UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = dept.Id };
            await _context.Students.AddAsync(student);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();
            
            var enrollment = new Enrollment { StudentId = student.Id, SectionId = section.Id, Section = section };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.ApproveEnrollmentAsync(enrollment.Id, instructor.Id); // Instructor doesn't own section

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
            
            var section = new CourseSection { InstructorId = instructor.Id, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30 };
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
            var dept = new Department { Name = "CS", Code = "CS" };
            await _context.Departments.AddAsync(dept);
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            await _context.Faculties.AddAsync(instructor);
            await _context.SaveChangesAsync();
            
            var section = new CourseSection { InstructorId = instructor.Id, Capacity = 10, EnrolledCount = 10, SectionNumber = "1", Semester = "Fall", Year = 2024, Instructor = instructor };
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
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            await _context.Faculties.AddAsync(instructor);
            await _context.SaveChangesAsync();
            
            var section = new CourseSection { InstructorId = instructor.Id, Capacity = 10, EnrolledCount = 0, SectionNumber = "1", Semester = "Fall", Year = 2024, Instructor = instructor };
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
            // Arrange
            var dept = new Department { Name = "CS", Code = "CS" };
            await _context.Departments.AddAsync(dept);
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            await _context.Faculties.AddAsync(instructor);
            await _context.SaveChangesAsync();
            
            var section = new CourseSection { InstructorId = instructor.Id + 1, SectionNumber = "1", Semester = "Fall", Year = 2024, Capacity = 30 }; // Different Instructor
            var stdUser = new User { Id = "s1", FullName = "Student" };
            var student = new Student { UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = dept.Id };
            await _context.Students.AddAsync(student);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();
            
            var enrollment = new Enrollment { StudentId = student.Id, SectionId = section.Id, Section = section };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.RejectEnrollmentAsync(enrollment.Id, instructor.Id, null);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task RejectEnrollmentAsync_ShouldFail_WhenNotPending()
        {
             // Arrange
            var dept = new Department { Name = "CS", Code = "CS" };
            await _context.Departments.AddAsync(dept);
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            await _context.Faculties.AddAsync(instructor);
            await _context.SaveChangesAsync();
            
            var section = new CourseSection { InstructorId = instructor.Id, SectionNumber = "1", Semester = "Fall", Year = 2024, Instructor = instructor, Capacity = 30 };
            var stdUser = new User { Id = "s1", FullName = "Student" };
            var student = new Student { UserId = "s1", User = stdUser, StudentNumber = "123", DepartmentId = dept.Id };
            await _context.Students.AddAsync(student);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();
            
            var enrollment = new Enrollment { StudentId = student.Id, SectionId = section.Id, Section = section, Status = EnrollmentStatus.Enrolled };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.RejectEnrollmentAsync(enrollment.Id, instructor.Id, null);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task RejectEnrollmentAsync_ShouldSucceed()
        {
            // Arrange
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            var section = new CourseSection { Id = 1, InstructorId = 1, SectionNumber = "1", Semester = "Fall", Year = 2024, Instructor = instructor, Capacity = 30 };
            await _context.Faculties.AddAsync(instructor);
            var dept = new Department { Id = 1, Name = "CS" };
            var stdUser = new User { Id = "s1", FullName = "Student" };
            var student = new Student { Id = 1, UserId = "s1", User = stdUser, StudentNumber = "123", Department = dept };
            await _context.Departments.AddAsync(dept);
            await _context.Students.AddAsync(student);
            var enrollment = new Enrollment { Id = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Pending };
            
            await _context.CourseSections.AddAsync(section);
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.RejectEnrollmentAsync(1, 1, "Full");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            enrollment.Status.Should().Be(EnrollmentStatus.Rejected);
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
            var course = new Course { Id = 1, Code = "C1", Name = "C1" };
            var instructor = new Faculty { User = new User { FullName = "I" }, EmployeeNumber = "EMP001", Title = "Prof" };
            var section = new CourseSection { Id = 1, InstructorId = 1, Course = course, SectionNumber = "1", Semester = "F", Year = 2024, Instructor = instructor, Capacity = 30 };
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
        
        // Removed tests for CheckPrerequisitesAsync and CheckScheduleConflictAsync as they are covered via EnrollInCourseAsync integration scenarios
        // or effectively tested implicitly. Added specific scenarios in EnrollInCourseAsync above.
    }
}
