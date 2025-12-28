using System.Collections.Generic;
using FluentAssertions;
using FluentValidation.TestHelper;
using SMARTCAMPUS.BusinessLayer.ValidationRules.Academic;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using Xunit;

namespace SMARTCAMPUS.Tests.ValidationRules
{
    public class ValidatorTests
    {
        #region AttendanceCheckInValidator Tests

        [Fact]
        public void AttendanceCheckInValidator_ShouldHaveError_WhenCoordinatesInvalid()
        {
            var validator = new AttendanceCheckInValidator();
            var dto = new AttendanceCheckInDto { Latitude = 200, Longitude = 200 }; // Invalid

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Latitude);
            result.ShouldHaveValidationErrorFor(x => x.Longitude);
        }

        [Fact]
        public void AttendanceCheckInValidator_ShouldNotHaveError_WhenCoordinatesValid()
        {
            var validator = new AttendanceCheckInValidator();
            var dto = new AttendanceCheckInDto { Latitude = 41.0082m, Longitude = 28.9784m }; // Istanbul

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Latitude);
            result.ShouldNotHaveValidationErrorFor(x => x.Longitude);
        }

        [Fact]
        public void AttendanceCheckInValidator_ShouldHaveError_WhenLatitudeOutOfRange()
        {
            var validator = new AttendanceCheckInValidator();
            var dto = new AttendanceCheckInDto { Latitude = -100, Longitude = 0 }; // Invalid latitude

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Latitude);
        }

        [Fact]
        public void AttendanceCheckInValidator_ShouldHaveError_WhenLongitudeOutOfRange()
        {
            var validator = new AttendanceCheckInValidator();
            var dto = new AttendanceCheckInDto { Latitude = 0, Longitude = 200 }; // Invalid longitude

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Longitude);
        }

        #endregion

        #region CourseCreateValidator Tests

        [Fact]
        public void CourseCreateValidator_ShouldHaveError_WhenCodeEmpty()
        {
            var validator = new CourseCreateValidator();
            var dto = new CourseCreateDto { Code = "", Name = "Name", DepartmentId = 1 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public void CourseCreateValidator_ShouldHaveError_WhenNameEmpty()
        {
            var validator = new CourseCreateValidator();
            var dto = new CourseCreateDto { Code = "CS101", Name = "", DepartmentId = 1 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void CourseCreateValidator_ShouldHaveError_WhenDepartmentIdInvalid()
        {
            var validator = new CourseCreateValidator();
            var dto = new CourseCreateDto { Code = "CS101", Name = "Intro", DepartmentId = 0 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
        }

        [Fact]
        public void CourseCreateValidator_ShouldNotHaveError_WhenAllFieldsValid()
        {
            var validator = new CourseCreateValidator();
            var dto = new CourseCreateDto { Code = "CS101", Name = "Introduction to CS", DepartmentId = 1, Credits = 3 };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Code);
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
            result.ShouldNotHaveValidationErrorFor(x => x.DepartmentId);
        }

        #endregion

        #region CourseSectionCreateValidator Tests

        [Fact]
        public void CourseSectionCreateValidator_ShouldHaveError_WhenSemesterEmpty()
        {
            var validator = new CourseSectionCreateValidator();
            var dto = new CourseSectionCreateDto { CourseId = 1, InstructorId = "1", Semester = "", Year = 2024 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Semester);
        }

        [Fact]
        public void CourseSectionCreateValidator_ShouldHaveError_WhenCourseIdInvalid()
        {
            var validator = new CourseSectionCreateValidator();
            var dto = new CourseSectionCreateDto { CourseId = 0, InstructorId = "1", Semester = "Fall", Year = 2024 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.CourseId);
        }

        [Fact]
        public void CourseSectionCreateValidator_ShouldNotHaveError_WhenAllFieldsValid()
        {
            var validator = new CourseSectionCreateValidator();
            var dto = new CourseSectionCreateDto { CourseId = 1, InstructorId = "1", Semester = "Fall", Year = 2024, Capacity = 30 };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.CourseId);
            result.ShouldNotHaveValidationErrorFor(x => x.Semester);
        }

        #endregion

        #region CourseSectionUpdateValidator Tests

        [Fact]
        public void CourseSectionUpdateValidator_ShouldHaveError_WhenSectionNumberTooLong()
        {
            var validator = new CourseSectionUpdateValidator();
            var dto = new CourseSectionUpdateDto { SectionNumber = "12345678901" }; // >10 chars

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SectionNumber);
        }

        [Fact]
        public void CourseSectionUpdateValidator_ShouldNotHaveError_WhenSectionNumberValid()
        {
            var validator = new CourseSectionUpdateValidator();
            var dto = new CourseSectionUpdateDto { SectionNumber = "A001" };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.SectionNumber);
        }

        [Fact]
        public void CourseSectionUpdateValidator_ShouldNotHaveError_WhenSectionNumberEmpty()
        {
            var validator = new CourseSectionUpdateValidator();
            var dto = new CourseSectionUpdateDto { SectionNumber = "" };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.SectionNumber);
        }

        [Fact]
        public void CourseSectionUpdateValidator_ShouldHaveError_WhenSemesterTooLong()
        {
            var validator = new CourseSectionUpdateValidator();
            var dto = new CourseSectionUpdateDto { Semester = "ThisIsAVeryLongSemesterName" }; // >20 chars

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Semester);
        }

        [Fact]
        public void CourseSectionUpdateValidator_ShouldNotHaveError_WhenSemesterValid()
        {
            var validator = new CourseSectionUpdateValidator();
            var dto = new CourseSectionUpdateDto { Semester = "Fall 2024" };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Semester);
        }

        [Fact]
        public void CourseSectionUpdateValidator_ShouldHaveError_WhenYearTooLow()
        {
            var validator = new CourseSectionUpdateValidator();
            var dto = new CourseSectionUpdateDto { Year = 1999 }; // <2000

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Year);
        }

        [Fact]
        public void CourseSectionUpdateValidator_ShouldHaveError_WhenYearTooHigh()
        {
            var validator = new CourseSectionUpdateValidator();
            var dto = new CourseSectionUpdateDto { Year = 2101 }; // >2100

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Year);
        }

        [Fact]
        public void CourseSectionUpdateValidator_ShouldNotHaveError_WhenYearValid()
        {
            var validator = new CourseSectionUpdateValidator();
            var dto = new CourseSectionUpdateDto { Year = 2024 };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Year);
        }

        [Fact]
        public void CourseSectionUpdateValidator_ShouldNotHaveError_WhenYearNull()
        {
            var validator = new CourseSectionUpdateValidator();
            var dto = new CourseSectionUpdateDto { Year = null };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Year);
        }

        [Fact]
        public void CourseSectionUpdateValidator_ShouldHaveError_WhenCapacityZeroOrNegative()
        {
            var validator = new CourseSectionUpdateValidator();
            var dto = new CourseSectionUpdateDto { Capacity = 0 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Capacity);
        }

        [Fact]
        public void CourseSectionUpdateValidator_ShouldHaveError_WhenCapacityOverLimit()
        {
            var validator = new CourseSectionUpdateValidator();
            var dto = new CourseSectionUpdateDto { Capacity = 501 }; // >500

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Capacity);
        }

        [Fact]
        public void CourseSectionUpdateValidator_ShouldNotHaveError_WhenCapacityValid()
        {
            var validator = new CourseSectionUpdateValidator();
            var dto = new CourseSectionUpdateDto { Capacity = 30 };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Capacity);
        }

        [Fact]
        public void CourseSectionUpdateValidator_ShouldNotHaveError_WhenCapacityNull()
        {
            var validator = new CourseSectionUpdateValidator();
            var dto = new CourseSectionUpdateDto { Capacity = null };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Capacity);
        }

        [Fact]
        public void CourseSectionUpdateValidator_ShouldHaveError_WhenScheduleJsonTooLong()
        {
            var validator = new CourseSectionUpdateValidator();
            var dto = new CourseSectionUpdateDto { ScheduleJson = new string('x', 2001) }; // >2000 chars

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ScheduleJson);
        }

        [Fact]
        public void CourseSectionUpdateValidator_ShouldNotHaveError_WhenScheduleJsonValid()
        {
            var validator = new CourseSectionUpdateValidator();
            var dto = new CourseSectionUpdateDto { ScheduleJson = "[{\"Day\":\"Monday\"}]" };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.ScheduleJson);
        }

        #endregion

        #region EnrollmentRequestValidator Tests

        [Fact]
        public void EnrollmentRequestValidator_ShouldHaveError_WhenSectionIdZero()
        {
            var validator = new EnrollmentRequestValidator();
            var dto = new EnrollmentRequestDto { SectionId = 0 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SectionId);
        }

        [Fact]
        public void EnrollmentRequestValidator_ShouldHaveError_WhenSectionIdNegative()
        {
            var validator = new EnrollmentRequestValidator();
            var dto = new EnrollmentRequestDto { SectionId = -1 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SectionId);
        }

        [Fact]
        public void EnrollmentRequestValidator_ShouldNotHaveError_WhenSectionIdValid()
        {
            var validator = new EnrollmentRequestValidator();
            var dto = new EnrollmentRequestDto { SectionId = 1 };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.SectionId);
        }

        #endregion

        #region GradeUpdateValidator Tests

        [Fact]
        public void GradeUpdateValidator_ShouldHaveError_WhenEnrollmentIdZero()
        {
            var validator = new GradeUpdateValidator();
            var dto = new GradeUpdateDto { EnrollmentId = 0, MidtermGrade = 80 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.EnrollmentId);
        }

        [Fact]
        public void GradeUpdateValidator_ShouldHaveError_WhenEnrollmentIdNegative()
        {
            var validator = new GradeUpdateValidator();
            var dto = new GradeUpdateDto { EnrollmentId = -1, MidtermGrade = 80 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.EnrollmentId);
        }

        [Fact]
        public void GradeUpdateValidator_ShouldHaveError_WhenMidtermGradeNegative()
        {
            var validator = new GradeUpdateValidator();
            var dto = new GradeUpdateDto { EnrollmentId = 1, MidtermGrade = -1 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.MidtermGrade);
        }

        [Fact]
        public void GradeUpdateValidator_ShouldHaveError_WhenMidtermGradeOver100()
        {
            var validator = new GradeUpdateValidator();
            var dto = new GradeUpdateDto { EnrollmentId = 1, MidtermGrade = 101 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.MidtermGrade);
        }

        [Fact]
        public void GradeUpdateValidator_ShouldHaveError_WhenFinalGradeNegative()
        {
            var validator = new GradeUpdateValidator();
            var dto = new GradeUpdateDto { EnrollmentId = 1, FinalGrade = -5 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.FinalGrade);
        }

        [Fact]
        public void GradeUpdateValidator_ShouldHaveError_WhenFinalGradeOver100()
        {
            var validator = new GradeUpdateValidator();
            var dto = new GradeUpdateDto { EnrollmentId = 1, FinalGrade = 150 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.FinalGrade);
        }

        [Fact]
        public void GradeUpdateValidator_ShouldHaveError_WhenNoGradeProvided()
        {
            var validator = new GradeUpdateValidator();
            var dto = new GradeUpdateDto { EnrollmentId = 1, MidtermGrade = null, FinalGrade = null };

            var result = validator.TestValidate(dto);

            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(x => x.ErrorMessage == "At least one grade (Midterm or Final) must be provided");
        }

        [Fact]
        public void GradeUpdateValidator_ShouldNotHaveError_WhenOnlyMidtermProvided()
        {
            var validator = new GradeUpdateValidator();
            var dto = new GradeUpdateDto { EnrollmentId = 1, MidtermGrade = 85, FinalGrade = null };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.EnrollmentId);
            result.ShouldNotHaveValidationErrorFor(x => x.MidtermGrade);
        }

        [Fact]
        public void GradeUpdateValidator_ShouldNotHaveError_WhenOnlyFinalProvided()
        {
            var validator = new GradeUpdateValidator();
            var dto = new GradeUpdateDto { EnrollmentId = 1, MidtermGrade = null, FinalGrade = 90 };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.EnrollmentId);
            result.ShouldNotHaveValidationErrorFor(x => x.FinalGrade);
        }

        [Fact]
        public void GradeUpdateValidator_ShouldNotHaveError_WhenBothGradesProvided()
        {
            var validator = new GradeUpdateValidator();
            var dto = new GradeUpdateDto { EnrollmentId = 1, MidtermGrade = 80, FinalGrade = 85 };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.EnrollmentId);
            result.ShouldNotHaveValidationErrorFor(x => x.MidtermGrade);
            result.ShouldNotHaveValidationErrorFor(x => x.FinalGrade);
        }

        [Fact]
        public void GradeUpdateValidator_ShouldNotHaveError_WhenGradesAtBoundary()
        {
            var validator = new GradeUpdateValidator();
            var dto = new GradeUpdateDto { EnrollmentId = 1, MidtermGrade = 0, FinalGrade = 100 };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.MidtermGrade);
            result.ShouldNotHaveValidationErrorFor(x => x.FinalGrade);
        }

        #endregion

        #region GradeBulkUpdateValidator Tests

        [Fact]
        public void GradeBulkUpdateValidator_ShouldHaveError_WhenGradesEmpty()
        {
            var validator = new GradeBulkUpdateValidator();
            var dto = new GradeBulkUpdateDto { Grades = new List<GradeUpdateDto>() };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Grades);
        }

        [Fact]
        public void GradeBulkUpdateValidator_ShouldHaveError_WhenGradesNull()
        {
            var validator = new GradeBulkUpdateValidator();
            var dto = new GradeBulkUpdateDto { Grades = null! };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Grades);
        }

        [Fact]
        public void GradeBulkUpdateValidator_ShouldValidateNestedGrades()
        {
            var validator = new GradeBulkUpdateValidator();
            var dto = new GradeBulkUpdateDto
            {
                Grades = new List<GradeUpdateDto>
                {
                    new GradeUpdateDto { EnrollmentId = 0, MidtermGrade = 80 } // Invalid EnrollmentId
                }
            };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor("Grades[0].EnrollmentId");
        }

        [Fact]
        public void GradeBulkUpdateValidator_ShouldNotHaveError_WhenAllGradesValid()
        {
            var validator = new GradeBulkUpdateValidator();
            var dto = new GradeBulkUpdateDto
            {
                Grades = new List<GradeUpdateDto>
                {
                    new GradeUpdateDto { EnrollmentId = 1, MidtermGrade = 80 },
                    new GradeUpdateDto { EnrollmentId = 2, FinalGrade = 90 }
                }
            };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Grades);
        }

        [Fact]
        public void GradeBulkUpdateValidator_ShouldValidateMultipleNestedErrors()
        {
            var validator = new GradeBulkUpdateValidator();
            var dto = new GradeBulkUpdateDto
            {
                Grades = new List<GradeUpdateDto>
                {
                    new GradeUpdateDto { EnrollmentId = 1, MidtermGrade = 150 }, // Invalid grade
                    new GradeUpdateDto { EnrollmentId = -1, FinalGrade = 50 }    // Invalid enrollment
                }
            };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor("Grades[0].MidtermGrade");
            result.ShouldHaveValidationErrorFor("Grades[1].EnrollmentId");
        }

        #endregion

        #region AttendanceSessionValidator Tests

        [Fact]
        public void AttendanceSessionValidator_ShouldHaveError_WhenSectionIdZero()
        {
            var validator = new AttendanceSessionValidator();
            var dto = new AttendanceSessionDto { SectionId = 0, Date = DateTime.Today, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10) };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SectionId);
        }

        [Fact]
        public void AttendanceSessionValidator_ShouldHaveError_WhenSectionIdNegative()
        {
            var validator = new AttendanceSessionValidator();
            var dto = new AttendanceSessionDto { SectionId = -1, Date = DateTime.Today, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10) };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SectionId);
        }

        [Fact]
        public void AttendanceSessionValidator_ShouldHaveError_WhenEndTimeBeforeStartTime()
        {
            var validator = new AttendanceSessionValidator();
            var dto = new AttendanceSessionDto { SectionId = 1, Date = DateTime.Today, StartTime = TimeSpan.FromHours(10), EndTime = TimeSpan.FromHours(9) };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.EndTime);
        }

        [Fact]
        public void AttendanceSessionValidator_ShouldHaveError_WhenLatitudeOutOfRange()
        {
            var validator = new AttendanceSessionValidator();
            var dto = new AttendanceSessionDto { SectionId = 1, Date = DateTime.Today, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10), Latitude = 100 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Latitude);
        }

        [Fact]
        public void AttendanceSessionValidator_ShouldHaveError_WhenLongitudeOutOfRange()
        {
            var validator = new AttendanceSessionValidator();
            var dto = new AttendanceSessionDto { SectionId = 1, Date = DateTime.Today, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10), Longitude = 200 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Longitude);
        }

        [Fact]
        public void AttendanceSessionValidator_ShouldHaveError_WhenGeofenceRadiusZeroOrNegative()
        {
            var validator = new AttendanceSessionValidator();
            var dto = new AttendanceSessionDto { SectionId = 1, Date = DateTime.Today, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10), GeofenceRadius = 0 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.GeofenceRadius);
        }

        [Fact]
        public void AttendanceSessionValidator_ShouldNotHaveError_WhenAllFieldsValid()
        {
            var validator = new AttendanceSessionValidator();
            var dto = new AttendanceSessionDto 
            { 
                SectionId = 1, 
                Date = DateTime.Today, 
                StartTime = TimeSpan.FromHours(9), 
                EndTime = TimeSpan.FromHours(10),
                Latitude = 41.0082m,
                Longitude = 28.9784m,
                GeofenceRadius = 15
            };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.SectionId);
            result.ShouldNotHaveValidationErrorFor(x => x.EndTime);
        }

        #endregion

        #region CourseUpdateValidator Tests

        [Fact]
        public void CourseUpdateValidator_ShouldHaveError_WhenNameTooLong()
        {
            var validator = new CourseUpdateValidator();
            var dto = new CourseUpdateDto { Name = new string('x', 201) }; // > 200 chars

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void CourseUpdateValidator_ShouldHaveError_WhenDescriptionTooLong()
        {
            var validator = new CourseUpdateValidator();
            var dto = new CourseUpdateDto { Description = new string('x', 2001) }; // > 2000 chars

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void CourseUpdateValidator_ShouldHaveError_WhenCreditsZeroOrNegative()
        {
            var validator = new CourseUpdateValidator();
            var dto = new CourseUpdateDto { Credits = 0 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Credits);
        }

        [Fact]
        public void CourseUpdateValidator_ShouldHaveError_WhenCreditsOver10()
        {
            var validator = new CourseUpdateValidator();
            var dto = new CourseUpdateDto { Credits = 11 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Credits);
        }

        [Fact]
        public void CourseUpdateValidator_ShouldHaveError_WhenECTSZeroOrNegative()
        {
            var validator = new CourseUpdateValidator();
            var dto = new CourseUpdateDto { ECTS = 0 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ECTS);
        }

        [Fact]
        public void CourseUpdateValidator_ShouldHaveError_WhenECTSOver30()
        {
            var validator = new CourseUpdateValidator();
            var dto = new CourseUpdateDto { ECTS = 31 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ECTS);
        }

        [Fact]
        public void CourseUpdateValidator_ShouldHaveError_WhenDepartmentIdZeroOrNegative()
        {
            var validator = new CourseUpdateValidator();
            var dto = new CourseUpdateDto { DepartmentId = 0 };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
        }

        [Fact]
        public void CourseUpdateValidator_ShouldHaveError_WhenSyllabusUrlTooLong()
        {
            var validator = new CourseUpdateValidator();
            var dto = new CourseUpdateDto { SyllabusUrl = new string('x', 501) }; // > 500 chars

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SyllabusUrl);
        }

        [Fact]
        public void CourseUpdateValidator_ShouldNotHaveError_WhenAllFieldsNullOrValid()
        {
            var validator = new CourseUpdateValidator();
            var dto = new CourseUpdateDto 
            { 
                Name = "Valid Course Name",
                Description = "A valid description",
                Credits = 3,
                ECTS = 5,
                DepartmentId = 1,
                SyllabusUrl = "http://example.com/syllabus"
            };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Name);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
            result.ShouldNotHaveValidationErrorFor(x => x.Credits);
            result.ShouldNotHaveValidationErrorFor(x => x.ECTS);
            result.ShouldNotHaveValidationErrorFor(x => x.DepartmentId);
            result.ShouldNotHaveValidationErrorFor(x => x.SyllabusUrl);
        }

        #endregion
    }
}
