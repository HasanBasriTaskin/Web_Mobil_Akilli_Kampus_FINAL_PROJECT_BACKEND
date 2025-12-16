using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Course;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface ICourseService
    {
        Task<Response<IEnumerable<CourseListDto>>> GetAllCoursesAsync(int page, int pageSize, int? departmentId = null, string? search = null);
        Task<Response<CourseDto>> GetCourseByIdAsync(int id);
        Task<Response<CourseDto>> CreateCourseAsync(CreateCourseDto dto);
        Task<Response<CourseDto>> UpdateCourseAsync(int id, UpdateCourseDto dto);
        Task<Response<NoDataDto>> DeleteCourseAsync(int id);
        Task<Response<IEnumerable<CoursePrerequisiteDto>>> GetPrerequisitesAsync(int courseId);
    }
}
