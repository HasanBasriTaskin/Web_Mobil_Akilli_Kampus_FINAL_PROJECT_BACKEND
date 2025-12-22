using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface ICourseDal : IGenericDal<Course>
    {
        Task<Course?> GetCourseWithPrerequisitesAsync(int id);
        Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(int departmentId);
        Task<List<Course>> GetAllCoursesWithDetailsAsync(int page, int pageSize, int? departmentId = null, string? search = null);
        Task<Course?> GetByIdWithDetailsAsync(int id);
    }
}
