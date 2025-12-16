using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IExcuseRequestDal : IGenericDal<ExcuseRequest>
    {
        Task<IEnumerable<ExcuseRequest>> GetRequestsByStudentAsync(int studentId);
        Task<IEnumerable<ExcuseRequest>> GetRequestsBySessionAsync(int sessionId);
        Task<IEnumerable<ExcuseRequest>> GetPendingRequestsAsync();
    }
}
