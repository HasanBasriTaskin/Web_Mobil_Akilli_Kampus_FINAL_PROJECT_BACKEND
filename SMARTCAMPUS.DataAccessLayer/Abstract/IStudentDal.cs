using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IStudentDal : IGenericDal<Student>
    {
        // Custom student queries can be added here
        // Task<Student> GetStudentWithDetailsAsync(int id);
    }
}
