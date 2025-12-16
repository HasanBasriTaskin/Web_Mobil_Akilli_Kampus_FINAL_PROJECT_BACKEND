using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs.Enrollment;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class EnrollmentManagerTests : IDisposable
    {
        private readonly Mock<IEnrollmentDal> _mockEnrollmentDal;
        private readonly Mock<ICourseSectionDal> _mockSectionDal;
        private readonly Mock<ICoursePrerequisiteDal> _mockPrerequisiteDal;
        private readonly CampusContext _context;
        private readonly EnrollmentManager _manager;

        public EnrollmentManagerTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _mockEnrollmentDal = new Mock<IEnrollmentDal>();
            _mockSectionDal = new Mock<ICourseSectionDal>();
            _mockPrerequisiteDal = new Mock<ICoursePrerequisiteDal>();

            _manager = new EnrollmentManager(_mockEnrollmentDal.Object, _mockSectionDal.Object, _mockPrerequisiteDal.Object, _context);
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
            // Arrange
            _mockSectionDal.Setup(x => x.GetSectionWithDetailsAsync(1)).ReturnsAsync((CourseSection?)null);

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
            var section = new CourseSection
            {
                Id = 1,
                EnrolledCount = 10,
                Capacity = 10,
                Course = new Course { Code = "C1", Name = "C1" },
                SectionNumber = "1",
                Semester = "Fall",
                Year = 2024
            };
            _mockSectionDal.Setup(x => x.GetSectionWithDetailsAsync(1)).ReturnsAsync(section);

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
            var section = new CourseSection
            {
                Id = 1,
                CourseId = 1,
                EnrolledCount = 0,
                Capacity = 10,
                Course = new Course { Code = "C1", Name = "C1" }
            };

            _mockSectionDal.Setup(x => x.GetSectionWithDetailsAsync(1)).ReturnsAsync(section);

            // Simulating missing prereqs
            _mockPrerequisiteDal.Setup(x => x.GetAllPrerequisiteIdsRecursiveAsync(1))
                .ReturnsAsync(new List<int> { 2 });

            var missingCourse = new Course { Id = 2, Code = "C2", Name = "C2" };
            await _context.Courses.AddAsync(missingCourse);
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
            var section = new CourseSection
            {
                Id = 2,
                CourseId = 2,
                EnrolledCount = 0,
                Capacity = 10,
                Course = new Course { Code = "C2", Name = "C2" },
                ScheduleJson = "[{\"Day\":\"Monday\",\"StartTime\":\"09:00\",\"EndTime\":\"10:00\"}]",
                SectionNumber = "2",
                Semester = "Fall",
                Year = 2024
            };

            // Existing enrollment with conflict
            var existingSection = new CourseSection
            {
                Id = 1,
                CourseId = 1,
                ScheduleJson = "[{\"Day\":\"Monday\",\"StartTime\":\"09:30\",\"EndTime\":\"10:30\"}]",
                Course = new Course { Code = "C1", Name = "C1" },
                SectionNumber = "1",
                Semester = "Fall",
                Year = 2024
            };
            var existingEnrollment = new Enrollment
            {
                StudentId = 1,
                SectionId = 1,
                Section = existingSection,
                Status = EnrollmentStatus.Enrolled
            };

            await _context.CourseSections.AddAsync(section);
            await _context.Enrollments.AddAsync(existingEnrollment);
            await _context.SaveChangesAsync();

            _mockSectionDal.Setup(x => x.GetSectionWithDetailsAsync(2)).ReturnsAsync(section);
            _mockPrerequisiteDal.Setup(x => x.GetAllPrerequisiteIdsRecursiveAsync(2)).ReturnsAsync(new List<int>());

            // Act
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
            var section = new CourseSection
            {
                Id = 2,
                CourseId = 1, // Same course ID as existing enrollment
                EnrolledCount = 0,
                Capacity = 10,
                Course = new Course { Id = 1, Code = "C1", Name = "C1" },
                SectionNumber = "2",
                Semester = "Fall",
                Year = 2024
            };

            var existingSection = new CourseSection
            {
                Id = 1,
                CourseId = 1,
                Course = section.Course,
                SectionNumber = "1",
                Semester = "Fall",
                Year = 2024
            };
            var existingEnrollment = new Enrollment
            {
                StudentId = 1,
                SectionId = 1,
                Section = existingSection,
                Status = EnrollmentStatus.Enrolled
            };

            await _context.Enrollments.AddAsync(existingEnrollment);
            await _context.SaveChangesAsync();

            _mockSectionDal.Setup(x => x.GetSectionWithDetailsAsync(2)).ReturnsAsync(section);
            _mockPrerequisiteDal.Setup(x => x.GetAllPrerequisiteIdsRecursiveAsync(1)).ReturnsAsync(new List<int>());

            // Act
            var result = await _manager.EnrollInCourseAsync(1, new CreateEnrollmentDto { SectionId = 2 });

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
            var course = new Course { Id = 1, Code = "C1", Name = "C1" };
            var section = new CourseSection
            {
                Id = 1,
                EnrolledCount = 0,
                Capacity = 10,
                CourseId = 1,
                Course = course,
                Instructor = instructor,
                SectionNumber = "1",
                Semester = "Fall",
                Year = 2024
            };

            var existingEnrollment = new Enrollment
            {
                StudentId = 1,
                SectionId = 1,
                Status = EnrollmentStatus.Dropped, // Previously dropped
                EnrollmentDate = DateTime.UtcNow.AddDays(-10)
            };

            await _context.Users.AddAsync(user);
            await _context.Faculties.AddAsync(instructor); // Ensure instructor exists
            await _context.CourseSections.AddAsync(section);
            await _context.Enrollments.AddAsync(existingEnrollment);
            await _context.SaveChangesAsync();

            _mockSectionDal.Setup(x => x.GetSectionWithDetailsAsync(1)).ReturnsAsync(section);
            _mockPrerequisiteDal.Setup(x => x.GetAllPrerequisiteIdsRecursiveAsync(1)).ReturnsAsync(new List<int>());

            // Act
            var result = await _manager.EnrollInCourseAsync(1, new CreateEnrollmentDto { SectionId = 1 });

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            existingEnrollment.Status.Should().Be(EnrollmentStatus.Pending);
            _mockEnrollmentDal.Verify(x => x.Update(existingEnrollment), Times.Once);
        }

        [Fact]
        public async Task EnrollInCourseAsync_ShouldFail_WhenAlreadyEnrolledOrPendingSameSection()
        {
             // Arrange
            var section = new CourseSection
            {
                Id = 1,
                CourseId = 1,
                EnrolledCount = 0,
                Capacity = 10,
                Course = new Course { Id = 1, Code = "C1", Name = "C1" },
            };

            var existingEnrollment = new Enrollment
            {
                StudentId = 1,
                SectionId = 1,
                Status = EnrollmentStatus.Pending
            };

            await _context.Enrollments.AddAsync(existingEnrollment);
            await _context.SaveChangesAsync();

            _mockSectionDal.Setup(x => x.GetSectionWithDetailsAsync(1)).ReturnsAsync(section);
            _mockPrerequisiteDal.Setup(x => x.GetAllPrerequisiteIdsRecursiveAsync(1)).ReturnsAsync(new List<int>());

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
            var course = new Course { Id = 1, Code = "C1", Name = "C1" };
            var section = new CourseSection
            {
                Id = 1,
                EnrolledCount = 0,
                Capacity = 10,
                CourseId = 1,
                Course = course,
                Instructor = instructor,
                SectionNumber = "1",
                Semester = "Fall",
                Year = 2024
            };

            _mockSectionDal.Setup(x => x.GetSectionWithDetailsAsync(1)).ReturnsAsync(section);
            _mockPrerequisiteDal.Setup(x => x.GetAllPrerequisiteIdsRecursiveAsync(1)).ReturnsAsync(new List<int>());

            // Note: Section needs to exist in context for conflict check which queries _context.CourseSections
            await _context.Users.AddAsync(user); // User required for instructor
            // Instructor is nested in Section, so AddAsync(section) should add Instructor if tracked properly,
            // but we need to be careful.
            // InMemory doesn't enforce FKs but requires properties.
            // Adding course and section should be enough if objects are fully populated.

            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();

            _mockEnrollmentDal.Setup(x => x.AddAsync(It.IsAny<Enrollment>())).Returns(Task.CompletedTask);

            // Act
            var result = await _manager.EnrollInCourseAsync(1, new CreateEnrollmentDto { SectionId = 1 });

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
            _mockEnrollmentDal.Verify(x => x.AddAsync(It.IsAny<Enrollment>()), Times.Once);
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
            var section = new CourseSection
            {
                Id = 1,
                SectionNumber = "1",
                Semester = "Fall",
                Year = 2024,
                Course = new Course { Code = "C1", Name = "C1" }
            };
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Section = section, EnrollmentDate = DateTime.UtcNow };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.DropCourseAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            enrollment.Status.Should().Be(EnrollmentStatus.Dropped);
            _mockSectionDal.Verify(x => x.DecrementEnrolledCountAsync(1), Times.Once);
        }

        [Fact]
        public async Task DropCourseAsync_ShouldWithdraw_WhenAfter4Weeks()
        {
            // Arrange
            var section = new CourseSection
            {
                Id = 1,
                SectionNumber = "1",
                Semester = "Fall",
                Year = 2024,
                Course = new Course { Code = "C1", Name = "C1" }
            };
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Section = section, EnrollmentDate = DateTime.UtcNow.AddDays(-30) };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.DropCourseAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            enrollment.Status.Should().Be(EnrollmentStatus.Withdrawn);
            _mockSectionDal.Verify(x => x.DecrementEnrolledCountAsync(1), Times.Never);
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
            var section = new CourseSection { Id = 1, InstructorId = 2, SectionNumber = "1", Semester = "Fall", Year = 2024 }; // different instructor
            var enrollment = new Enrollment { Id = 1, SectionId = 1, Section = section };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.ApproveEnrollmentAsync(1, 1); // instructorId 1

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task ApproveEnrollmentAsync_ShouldFail_WhenNotPending()
        {
            // Arrange
            var section = new CourseSection { Id = 1, InstructorId = 1, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            var enrollment = new Enrollment { Id = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Enrolled };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.ApproveEnrollmentAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task ApproveEnrollmentAsync_ShouldFail_WhenSectionFull()
        {
            // Arrange
            var section = new CourseSection { Id = 1, InstructorId = 1, Capacity = 10, EnrolledCount = 10, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            var enrollment = new Enrollment { Id = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Pending };
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
            var section = new CourseSection
            {
                Id = 1,
                InstructorId = 1,
                Capacity = 10,
                EnrolledCount = 0,
                SectionNumber = "1",
                Semester = "Fall",
                Year = 2024,
                Course = new Course { Code = "C1", Name = "C1" }
            };
            var enrollment = new Enrollment { Id = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Pending };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.ApproveEnrollmentAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            enrollment.Status.Should().Be(EnrollmentStatus.Enrolled);
            _mockSectionDal.Verify(x => x.IncrementEnrolledCountAsync(1), Times.Once);
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
            var section = new CourseSection { Id = 1, InstructorId = 2, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            var enrollment = new Enrollment { Id = 1, SectionId = 1, Section = section };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.RejectEnrollmentAsync(1, 1, null);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task RejectEnrollmentAsync_ShouldFail_WhenNotPending()
        {
             // Arrange
            var section = new CourseSection { Id = 1, InstructorId = 1, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            var enrollment = new Enrollment { Id = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Enrolled };
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
            var section = new CourseSection
            {
                Id = 1,
                InstructorId = 1,
                SectionNumber = "1",
                Semester = "Fall",
                Year = 2024,
                Course = new Course { Code = "C1", Name = "C1" }
            };
            var enrollment = new Enrollment { Id = 1, SectionId = 1, Section = section, Status = EnrollmentStatus.Pending };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.RejectEnrollmentAsync(1, 1, "Full");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            enrollment.Status.Should().Be(EnrollmentStatus.Rejected);
        }

        #endregion

        #region CheckPrerequisitesAsync Tests

        [Fact]
        public async Task CheckPrerequisitesAsync_ShouldFail_WhenMissing()
        {
            // Arrange
            _mockPrerequisiteDal.Setup(x => x.GetAllPrerequisiteIdsRecursiveAsync(1)).ReturnsAsync(new List<int> { 2 });
            // Student has no completed courses
            var missingCourse = new Course { Id = 2, Code = "C2", Name = "C2" };
            await _context.Courses.AddAsync(missingCourse);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.CheckPrerequisitesAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Errors.Should().ContainMatch("*Missing prerequisites*C2*");
        }

        [Fact]
        public async Task CheckPrerequisitesAsync_ShouldSucceed_WhenCompleted()
        {
            // Arrange
            _mockPrerequisiteDal.Setup(x => x.GetAllPrerequisiteIdsRecursiveAsync(1)).ReturnsAsync(new List<int> { 2 });

            var instructorUser = new User { FullName = "Prof" };
            var instructor = new Faculty { Title = "Dr.", User = instructorUser, EmployeeNumber = "E1" };

            var studentUser = new User { FullName = "Student" };
            var student = new Student { Id = 1, StudentNumber = "S1", User = studentUser };

            var section = new CourseSection
            {
                CourseId = 2,
                SectionNumber = "1",
                Semester = "Fall",
                Year = 2024,
                Course = new Course { Id = 2, Code = "C2", Name = "C2" },
                Instructor = instructor
            };
            var enrollment = new Enrollment { StudentId = 1, Section = section, Status = EnrollmentStatus.Completed, Student = student };

            await _context.Users.AddRangeAsync(instructorUser, studentUser);
            await _context.Faculties.AddAsync(instructor);
            await _context.Students.AddAsync(student);
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.CheckPrerequisitesAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task CheckPrerequisitesAsync_ShouldSucceed_WhenNoPrerequisites()
        {
             // Arrange
            _mockPrerequisiteDal.Setup(x => x.GetAllPrerequisiteIdsRecursiveAsync(1)).ReturnsAsync(new List<int>());

            // Act
            var result = await _manager.CheckPrerequisitesAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        #endregion

        #region CheckScheduleConflictAsync Tests

        [Fact]
        public async Task CheckScheduleConflictAsync_ShouldSucceed_WhenNoConflict()
        {
            // Arrange
            var section = new CourseSection { Id = 1, SectionNumber = "1", Semester = "Fall", Year = 2024 };
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.CheckScheduleConflictAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

         [Fact]
        public async Task CheckScheduleConflictAsync_ShouldFail_WhenConflictExists()
        {
            // Arrange
            var section = new CourseSection
            {
                Id = 2,
                ScheduleJson = "[{\"Day\":\"Monday\",\"StartTime\":\"09:00\",\"EndTime\":\"10:00\"}]",
                SectionNumber = "2",
                Semester = "Fall",
                Year = 2024
            };

            var existingSection = new CourseSection
            {
                Id = 1,
                ScheduleJson = "[{\"Day\":\"Monday\",\"StartTime\":\"09:30\",\"EndTime\":\"10:30\"}]",
                Course = new Course { Code = "C1", Name = "C1" },
                SectionNumber = "1",
                Semester = "Fall",
                Year = 2024
            };
            var existingEnrollment = new Enrollment { StudentId = 1, Section = existingSection, Status = EnrollmentStatus.Enrolled };

            await _context.CourseSections.AddAsync(section);
            await _context.Enrollments.AddAsync(existingEnrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.CheckScheduleConflictAsync(1, 2);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Errors.Should().ContainMatch("*Schedule conflict*");
        }

        [Fact]
        public async Task CheckScheduleConflictAsync_ShouldHandleInvalidScheduleJson()
        {
             // Arrange
            var section = new CourseSection
            {
                Id = 2,
                ScheduleJson = "InvalidJson",
                SectionNumber = "2",
                Semester = "Fall",
                Year = 2024
            };

            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();

             // Act
            var result = await _manager.CheckScheduleConflictAsync(1, 2);

            // Assert
            result.IsSuccessful.Should().BeTrue(); // Should succeed if schedule is unparseable (fail safe) or treat as empty
        }

        #endregion

        #region GetMySectionsAsync Tests

        [Fact]
        public async Task GetMySectionsAsync_ShouldReturnSections()
        {
            // Arrange
            var course = new Course { Id = 1, Code = "C1", Name = "C1" };
            var section = new CourseSection { Id = 1, InstructorId = 1, Course = course, SectionNumber = "1", Semester = "F", Year = 2024 };
            await _context.Courses.AddAsync(course);
            await _context.CourseSections.AddAsync(section);
            await _context.SaveChangesAsync();

            // Act
            var result = await _manager.GetMySectionsAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            // Force execution
            var list = result.Data.ToList();
            list.Count.Should().Be(1);
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
            var section = new CourseSection { Id = 1, InstructorId = 1, Course = course, SectionNumber = "1", Semester = "F", Year = 2024 };
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
             // Force execution
            var list = result.Data.ToList();
            list.Count.Should().Be(1);
        }

        #endregion

        #region GetStudentsBySectionAsync Tests

        [Fact]
        public async Task GetStudentsBySectionAsync_ShouldFail_WhenSectionNotFound()
        {
             // Act
            var result = await _manager.GetStudentsBySectionAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetStudentsBySectionAsync_ShouldReturnStudents()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "Student" };
            var student = new Student { Id = 1, UserId = "u1", User = user, StudentNumber = "S1" };
            var section = new CourseSection { Id = 1, InstructorId = 1, SectionNumber = "1", Semester = "F", Year = 2024, Course = new Course { Code = "C1", Name = "C1" } };
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Status = EnrollmentStatus.Enrolled, Student = student, Section = section };

            await _context.Users.AddAsync(user);
            await _context.Students.AddAsync(student);
            await _context.CourseSections.AddAsync(section);
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            _mockEnrollmentDal.Setup(x => x.GetEnrollmentsBySectionAsync(1)).ReturnsAsync(new List<Enrollment> { enrollment });

            // Act
            var result = await _manager.GetStudentsBySectionAsync(1, 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
             // Force execution
            var list = result.Data.ToList();
            list.Count.Should().Be(1);
        }

        #endregion

        #region GetMyCoursesAsync Tests

        [Fact]
        public async Task GetMyCoursesAsync_ShouldReturnCourses()
        {
             // Arrange
            var user = new User { FullName = "Prof" };
            var instructor = new Faculty { Title = "Dr.", User = user, EmployeeNumber = "E1" };
            var section = new CourseSection { Id = 1, SectionNumber = "1", Semester = "F", Year = 2024, Course = new Course { Code = "C1", Name = "C1" }, Instructor = instructor };
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SectionId = 1, Status = EnrollmentStatus.Enrolled, Section = section };

            _mockEnrollmentDal.Setup(x => x.GetEnrollmentsByStudentAsync(1)).ReturnsAsync(new List<Enrollment> { enrollment });

            // Act
            var result = await _manager.GetMyCoursesAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
             // Force execution
            var list = result.Data.ToList();
            list.Count.Should().Be(1);
        }

        #endregion
    }
}
