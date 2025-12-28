using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfWalletTransactionDal : GenericRepository<WalletTransaction>, IWalletTransactionDal
    {
        public EfWalletTransactionDal(CampusContext context) : base(context)
        {
        }

        public async Task<List<WalletTransaction>> GetByWalletIdPagedAsync(int walletId, int page, int pageSize)
        {
            return await _context.WalletTransactions
                .Where(t => t.WalletId == walletId)
                .OrderByDescending(t => t.TransactionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountByWalletIdAsync(int walletId)
        {
            return await _context.WalletTransactions.CountAsync(t => t.WalletId == walletId);
        }
    }
}
