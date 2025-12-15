using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface ICourseSectionDal : IGenericDal<CourseSection>
    {
        Task<CourseSection?> GetSectionWithDetailsAsync(int id);
        Task<IEnumerable<CourseSection>> GetSectionsBySemesterAsync(string semester, int year);
        Task<IEnumerable<CourseSection>> GetSectionsByInstructorAsync(int instructorId);
        Task<bool> IncrementEnrolledCountAsync(int sectionId);
        Task<bool> DecrementEnrolledCountAsync(int sectionId);
    }
}
