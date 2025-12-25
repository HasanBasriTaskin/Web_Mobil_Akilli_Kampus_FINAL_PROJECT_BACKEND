using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfWalletDal : GenericRepository<Wallet>, IWalletDal
    {
        public EfWalletDal(CampusContext context) : base(context)
        {
        }

        public async Task<Wallet?> GetByUserIdAsync(string userId)
        {
            return await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<Wallet?> GetByUserIdWithTransactionsAsync(string userId, int page, int pageSize)
        {
            return await _context.Wallets
                .Include(w => w.Transactions
                    .OrderByDescending(t => t.TransactionDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize))
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }
    }
}
