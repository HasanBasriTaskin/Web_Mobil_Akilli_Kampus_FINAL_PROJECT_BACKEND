using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface ICourseSectionDal : IGenericDal<CourseSection>
    {
        Task<List<CourseSection>> GetSectionsByInstructorAsync(int instructorId);
        Task<CourseSection?> GetSectionWithDetailsAsync(int sectionId);
        Task IncrementEnrolledCountAsync(int sectionId);
        Task DecrementEnrolledCountAsync(int sectionId);
        Task<List<CourseSection>> GetSectionsBySemesterAsync(string semester, int year);
        Task<List<CourseSection>> GetSectionsByDepartmentAsync(int departmentId);
    }
}
