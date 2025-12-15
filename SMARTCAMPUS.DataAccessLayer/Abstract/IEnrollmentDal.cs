using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IEnrollmentDal : IGenericDal<Enrollment>
    {
        Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(int studentId);
        Task<IEnumerable<Enrollment>> GetEnrollmentsBySectionAsync(int sectionId);
        Task<Enrollment?> GetEnrollmentWithDetailsAsync(int id);
        Task<bool> HasStudentCompletedCourseAsync(int studentId, int courseId);
    }
}
