using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Wallet;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IWalletService
    {
        // Kullanıcı işlemleri
        Task<Response<WalletDto>> GetWalletAsync(string userId);
        Task<Response<PagedResponse<WalletTransactionDto>>> GetTransactionsAsync(string userId, int page = 1, int pageSize = 20);
        Task<Response<TopUpResultDto>> TopUpAsync(string userId, WalletTopUpDto dto);
        
        // Internal işlemler (diğer servisler tarafından kullanılır)
        Task<Response<WalletTransactionDto>> DeductAsync(string userId, decimal amount, ReferenceType referenceType, int? referenceId, string? description = null);
        Task<Response<WalletTransactionDto>> RefundAsync(string userId, decimal amount, ReferenceType referenceType, int? referenceId, string? description = null);
        
        // Admin işlemleri
        Task<Response<WalletDto>> GetWalletByUserIdAsync(string userId);
        Task<Response<NoDataDto>> SetWalletStatusAsync(string userId, bool isActive);
    }
}
