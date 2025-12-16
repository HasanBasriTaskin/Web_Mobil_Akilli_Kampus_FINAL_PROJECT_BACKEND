using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface ICoursePrerequisiteDal
    {
        Task<IEnumerable<CoursePrerequisite>> GetPrerequisitesForCourseAsync(int courseId);
        Task<IEnumerable<int>> GetAllPrerequisiteIdsRecursiveAsync(int courseId);
        Task AddAsync(CoursePrerequisite entity);
        Task RemoveAsync(int courseId, int prerequisiteCourseId);
        Task<bool> ExistsAsync(int courseId, int prerequisiteCourseId);
    }
}
