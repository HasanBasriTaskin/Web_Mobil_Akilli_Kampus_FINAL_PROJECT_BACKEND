using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IWalletTransactionDal : IGenericDal<WalletTransaction>
    {
        Task<List<WalletTransaction>> GetByWalletIdPagedAsync(int walletId, int page, int pageSize);
        Task<int> GetCountByWalletIdAsync(int walletId);
    }
}
