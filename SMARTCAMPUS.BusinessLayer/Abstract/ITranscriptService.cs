using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface ITranscriptService
    {
        Task<Response<TranscriptDto>> GetTranscriptAsync(int studentId);
        Task<Response<byte[]>> GenerateTranscriptPdfAsync(int studentId);
    }
}

