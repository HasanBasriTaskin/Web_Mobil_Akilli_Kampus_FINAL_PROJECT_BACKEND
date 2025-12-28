using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IWalletDal : IGenericDal<Wallet>
    {
        Task<Wallet?> GetByUserIdAsync(string userId);
        Task<Wallet?> GetByUserIdWithTransactionsAsync(string userId, int page, int pageSize);
    }
}
