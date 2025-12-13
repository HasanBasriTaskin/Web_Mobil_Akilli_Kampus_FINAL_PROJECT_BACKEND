using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IStudentDal : IGenericDal<Student>
    {
        Task<Student?> GetStudentWithDetailsAsync(int id);
    }
}
