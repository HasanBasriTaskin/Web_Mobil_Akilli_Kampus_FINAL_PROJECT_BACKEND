using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface ICourseService
    {
        Task<Response<PagedResponse<CourseDto>>> GetCoursesAsync(CourseQueryParameters queryParams);
        Task<Response<CourseDto>> GetCourseByIdAsync(int courseId);
        Task<Response<CourseDto>> GetCourseByCodeAsync(string code);
        Task<Response<IEnumerable<CourseDto>>> GetCoursesByDepartmentAsync(int departmentId);
        Task<Response<IEnumerable<CourseSectionDto>>> GetCourseSectionsAsync(int courseId);
        Task<Response<CourseSectionDto>> GetSectionByIdAsync(int sectionId);
        Task<Response<CourseDto>> CreateCourseAsync(CourseCreateDto courseCreateDto);
        Task<Response<CourseDto>> UpdateCourseAsync(int courseId, CourseUpdateDto courseUpdateDto);
        Task<Response<NoDataDto>> DeleteCourseAsync(int courseId);
    }
}



