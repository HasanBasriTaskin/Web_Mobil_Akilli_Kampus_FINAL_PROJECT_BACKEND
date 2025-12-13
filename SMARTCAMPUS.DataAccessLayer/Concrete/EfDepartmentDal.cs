using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfDepartmentDal : GenericRepository<Department>, IDepartmentDal
    {
        public EfDepartmentDal(CampusContext context) : base(context)
        {
        }
    }
}
