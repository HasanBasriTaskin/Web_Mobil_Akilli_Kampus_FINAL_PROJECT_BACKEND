using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IGradeService
    {
        Task<Response<IEnumerable<GradeDto>>> GetSectionGradesAsync(int sectionId);
        Task<Response<GradeDto>> GetStudentGradeAsync(int enrollmentId);
        Task<Response<IEnumerable<GradeDto>>> GetMyGradesAsync(int studentId);
        Task<Response<NoDataDto>> UpdateGradeAsync(int enrollmentId, GradeUpdateDto gradeUpdate);
        Task<Response<NoDataDto>> CreateGradeAsync(int enrollmentId, GradeUpdateDto gradeUpdate, string instructorId);
        Task<Response<NoDataDto>> BulkUpdateGradesAsync(int sectionId, GradeBulkUpdateDto grades);
        Task<Response<string>> CalculateLetterGradeAsync(decimal? midtermGrade, decimal? finalGrade);
        Task<Response<decimal>> CalculateGradePointAsync(string letterGrade);
        Task<Response<decimal>> CalculateGPAAsync(int studentId);
        Task<Response<decimal>> CalculateCGPAAsync(int studentId);
    }
}



