using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfPasswordResetTokenDal : GenericRepository<PasswordResetToken>, IPasswordResetTokenDal
    {
        public EfPasswordResetTokenDal(CampusContext context) : base(context)
        {
        }
    }
}
