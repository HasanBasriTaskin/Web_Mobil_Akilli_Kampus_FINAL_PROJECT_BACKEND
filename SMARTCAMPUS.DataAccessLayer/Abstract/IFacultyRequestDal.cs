using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IFacultyRequestDal : IGenericDal<FacultyCourseSectionRequest>
    {
        Task<List<FacultyCourseSectionRequest>> GetPendingRequestsAsync();
        Task<List<FacultyCourseSectionRequest>> GetRequestsByFacultyAsync(int facultyId);
        Task<FacultyCourseSectionRequest?> GetRequestWithDetailsAsync(int id);
        Task<bool> HasPendingRequestAsync(int facultyId, int sectionId);
    }
}
