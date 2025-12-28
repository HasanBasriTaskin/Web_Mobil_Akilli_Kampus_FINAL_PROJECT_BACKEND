using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IFacultyDal : IGenericDal<Faculty>
    {
        Task<Faculty?> GetByUserIdAsync(string userId);
        Task<Faculty?> GetFacultyWithUserAsync(int facultyId);
    }
}
