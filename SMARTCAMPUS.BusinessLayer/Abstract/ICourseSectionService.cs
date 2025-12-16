using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface ICourseSectionService
    {
        Task<Response<PagedResponse<CourseSectionDto>>> GetSectionsAsync(CourseSectionQueryParameters queryParams);
        Task<Response<CourseSectionDto>> GetSectionByIdAsync(int sectionId);
        Task<Response<CourseSectionDto>> CreateSectionAsync(CourseSectionCreateDto sectionCreateDto);
        Task<Response<CourseSectionDto>> UpdateSectionAsync(int sectionId, CourseSectionUpdateDto sectionUpdateDto);
    }
}

