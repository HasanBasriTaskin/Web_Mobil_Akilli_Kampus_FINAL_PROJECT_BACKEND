using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IEnrollmentDal : IGenericDal<Enrollment>
    {
        Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(int studentId);
        Task<IEnumerable<Enrollment>> GetEnrollmentsBySectionAsync(int sectionId);
        Task<Enrollment?> GetEnrollmentWithDetailsAsync(int id);
        Task<bool> HasStudentCompletedCourseAsync(int studentId, int courseId);
        Task<List<Enrollment>> GetPendingEnrollmentsAsync(int sectionId);
        Task<Enrollment?> GetByStudentAndSectionAsync(int studentId, int sectionId);
        Task<bool> IsEnrolledInOtherSectionAsync(int studentId, int courseId);
        Task<List<int>> GetCompletedCourseIdsAsync(int studentId);
        Task<List<int>> GetEnrolledSectionIdsAsync(int studentId);
    }
}
