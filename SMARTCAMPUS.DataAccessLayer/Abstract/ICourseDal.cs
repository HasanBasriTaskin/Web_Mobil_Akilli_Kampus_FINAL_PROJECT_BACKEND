using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface ICourseDal : IGenericDal<Course>
    {
        Task<Course?> GetCourseWithPrerequisitesAsync(int id);
        Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(int departmentId);
    }
}
