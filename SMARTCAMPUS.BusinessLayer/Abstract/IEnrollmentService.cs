using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IEnrollmentService
    {
        Task<Response<EnrollmentResponseDto>> EnrollAsync(int studentId, EnrollmentRequestDto request);
        Task<Response<NoDataDto>> DropCourseAsync(int studentId, int enrollmentId);
        Task<Response<IEnumerable<EnrollmentDto>>> GetStudentEnrollmentsAsync(int studentId);
        Task<Response<IEnumerable<EnrollmentDto>>> GetSectionEnrollmentsAsync(int sectionId);
        Task<Response<bool>> CheckPrerequisitesAsync(int courseId, int studentId);
        Task<Response<bool>> CheckScheduleConflictAsync(int studentId, int sectionId);
        Task<Response<PersonalScheduleDto>> GetPersonalScheduleAsync(int studentId, string? semester = null, int? year = null);
    }
}



