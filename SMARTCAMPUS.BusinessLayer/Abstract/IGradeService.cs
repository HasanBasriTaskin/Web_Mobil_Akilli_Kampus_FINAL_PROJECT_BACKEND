using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IGradeService
    {
        Task<Response<IEnumerable<GradeDto>>> GetSectionGradesAsync(int sectionId);
        Task<Response<GradeDto>> GetStudentGradeAsync(int enrollmentId);
        Task<Response<NoDataDto>> UpdateGradeAsync(int enrollmentId, GradeUpdateDto gradeUpdate);
        Task<Response<NoDataDto>> BulkUpdateGradesAsync(int sectionId, GradeBulkUpdateDto grades);
        Task<Response<string>> CalculateLetterGradeAsync(decimal? midtermGrade, decimal? finalGrade);
        Task<Response<decimal>> CalculateGradePointAsync(string letterGrade);
    }
}



