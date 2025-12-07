using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfEmailVerificationTokenDal : GenericRepository<EmailVerificationToken>, IEmailVerificationTokenDal
    {
        public EfEmailVerificationTokenDal(CampusContext context) : base(context)
        {
        }
    }
}
