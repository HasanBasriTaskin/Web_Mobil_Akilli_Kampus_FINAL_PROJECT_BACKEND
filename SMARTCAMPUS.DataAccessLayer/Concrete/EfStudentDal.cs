using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfStudentDal : GenericRepository<Student>, IStudentDal
    {
        public EfStudentDal(CampusContext context) : base(context)
        {
        }
    }
}
