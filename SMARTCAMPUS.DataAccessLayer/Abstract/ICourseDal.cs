using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface ICourseDal : IGenericDal<Course>
    {
        Task<Course?> GetCourseWithPrerequisitesAsync(int courseId);
        Task<Course?> GetCourseByCodeAsync(string code);
        Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(int departmentId);
        Task<bool> CheckPrerequisiteAsync(int courseId, int studentId);
    }
}



