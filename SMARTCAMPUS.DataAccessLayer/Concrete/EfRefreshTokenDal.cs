using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfRefreshTokenDal : GenericRepository<RefreshToken>, IRefreshTokenDal
    {
        public EfRefreshTokenDal(CampusContext context) : base(context)
        {
        }
    }
}
