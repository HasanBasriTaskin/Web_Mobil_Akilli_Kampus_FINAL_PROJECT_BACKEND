using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Enrollment;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IEnrollmentService
    {
        // Student operations
        Task<Response<EnrollmentDto>> EnrollInCourseAsync(int studentId, CreateEnrollmentDto dto);
        Task<Response<NoDataDto>> DropCourseAsync(int studentId, int enrollmentId);
        Task<Response<IEnumerable<StudentCourseDto>>> GetMyCoursesAsync(int studentId);
        
        // Faculty operations
        Task<Response<IEnumerable<SectionStudentDto>>> GetStudentsBySectionAsync(int sectionId, int instructorId);
        
        // Validation
        Task<Response<NoDataDto>> CheckPrerequisitesAsync(int studentId, int courseId);
        Task<Response<NoDataDto>> CheckScheduleConflictAsync(int studentId, int sectionId);
    }
}
