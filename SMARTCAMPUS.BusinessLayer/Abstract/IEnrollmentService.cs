using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IEnrollmentService
    {
        Task<Response<EnrollmentResponseDto>> EnrollAsync(int studentId, EnrollmentRequestDto request);
        Task<Response<NoDataDto>> DropCourseAsync(int studentId, int enrollmentId);
        Task<Response<IEnumerable<EnrollmentDto>>> GetStudentEnrollmentsAsync(int studentId, int? requestingStudentId = null, bool isAdmin = false, string? instructorId = null);
        Task<Response<IEnumerable<EnrollmentDto>>> GetSectionEnrollmentsAsync(int sectionId, string? instructorId = null, bool isAdmin = false);
        Task<Response<bool>> CheckPrerequisitesAsync(int courseId, int studentId);
        Task<Response<bool>> CheckScheduleConflictAsync(int studentId, int sectionId);
        Task<Response<PersonalScheduleDto>> GetPersonalScheduleAsync(int studentId, string? semester = null, int? year = null);
    }
}



