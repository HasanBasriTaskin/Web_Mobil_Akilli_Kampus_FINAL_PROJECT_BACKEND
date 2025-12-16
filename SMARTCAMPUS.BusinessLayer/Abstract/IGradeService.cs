using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Grade;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IGradeService
    {
        // Student operations
        Task<Response<IEnumerable<StudentGradeDto>>> GetMyGradesAsync(int studentId);
        Task<Response<TranscriptDto>> GetTranscriptAsync(int studentId);
        Task<byte[]> GenerateTranscriptPdfAsync(int studentId);
        
        // Faculty operations
        Task<Response<NoDataDto>> EnterGradeAsync(int instructorId, GradeEntryDto dto);
        Task<Response<NoDataDto>> EnterGradesBatchAsync(int instructorId, List<GradeEntryDto> dtos);
    }
}
