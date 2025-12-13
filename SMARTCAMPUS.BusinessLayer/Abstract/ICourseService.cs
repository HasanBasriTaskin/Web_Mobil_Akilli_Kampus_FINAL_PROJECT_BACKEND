using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface ICourseService
    {
        Task<Response<IEnumerable<CourseDto>>> GetCoursesAsync();
        Task<Response<CourseDto>> GetCourseByIdAsync(int courseId);
        Task<Response<CourseDto>> GetCourseByCodeAsync(string code);
        Task<Response<IEnumerable<CourseDto>>> GetCoursesByDepartmentAsync(int departmentId);
        Task<Response<IEnumerable<CourseSectionDto>>> GetCourseSectionsAsync(int courseId);
        Task<Response<CourseSectionDto>> GetSectionByIdAsync(int sectionId);
    }
}

