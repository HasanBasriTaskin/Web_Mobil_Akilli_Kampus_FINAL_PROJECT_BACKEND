using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IEnrollmentDal : IGenericDal<Enrollment>
    {
        Task<Enrollment?> GetEnrollmentWithDetailsAsync(int enrollmentId);
        Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(int studentId);
        Task<IEnumerable<Enrollment>> GetEnrollmentsBySectionAsync(int sectionId);
        Task<Enrollment?> GetEnrollmentByStudentAndSectionAsync(int studentId, int sectionId);
        Task<bool> IsEnrolledAsync(int studentId, int sectionId);
    }
}



