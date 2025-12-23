using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Wallet;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class WalletManager : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMockPaymentService _paymentService;

        public WalletManager(IUnitOfWork unitOfWork, IMockPaymentService paymentService)
        {
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
        }

        public async Task<Response<WalletDto>> GetWalletAsync(string userId)
        {
            var wallet = await GetOrCreateWalletAsync(userId);

            var dto = new WalletDto
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Balance = wallet.Balance,
                Currency = wallet.Currency,
                IsActive = wallet.IsActive
            };

            return Response<WalletDto>.Success(dto, 200);
        }

        public async Task<Response<PagedResponse<WalletTransactionDto>>> GetTransactionsAsync(string userId, int page = 1, int pageSize = 20)
        {
            var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
            if (wallet == null)
                return Response<PagedResponse<WalletTransactionDto>>.Fail("Cüzdan bulunamadı", 404);

            var totalCount = await _unitOfWork.WalletTransactions.GetCountByWalletIdAsync(wallet.Id);
            var transactions = await _unitOfWork.WalletTransactions.GetByWalletIdPagedAsync(wallet.Id, page, pageSize);

            var dtos = transactions.Select(t => new WalletTransactionDto
            {
                Id = t.Id,
                WalletId = t.WalletId,
                Type = t.Type,
                Amount = t.Amount,
                BalanceAfter = t.BalanceAfter,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                Description = t.Description,
                TransactionDate = t.TransactionDate
            }).ToList();

            var pagedResponse = new PagedResponse<WalletTransactionDto>(dtos, page, pageSize, totalCount);

            return Response<PagedResponse<WalletTransactionDto>>.Success(pagedResponse, 200);
        }

        public async Task<Response<TopUpResultDto>> TopUpAsync(string userId, WalletTopUpDto dto)
        {
            var paymentDto = new PaymentDto
            {
                CardNumber = dto.CardNumber,
                CVV = dto.CVV,
                ExpiryDate = dto.ExpiryDate,
                Amount = dto.Amount
            };

            var paymentResult = _paymentService.ProcessPayment(paymentDto);

            if (!paymentResult.IsSuccess)
            {
                return Response<TopUpResultDto>.Fail($"Ödeme başarısız: {paymentResult.ErrorMessage}", 400);
            }

            // ACID Transaction başlat
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var wallet = await GetOrCreateWalletAsync(userId);

                if (!wallet.IsActive)
                    return Response<TopUpResultDto>.Fail("Cüzdan aktif değil", 400);

                wallet.Balance += paymentDto.Amount;
                wallet.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.Wallets.Update(wallet);

                var walletTransaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Type = TransactionType.Credit,
                    Amount = paymentDto.Amount,
                    BalanceAfter = wallet.Balance,
                    ReferenceType = ReferenceType.TopUp,
                    Description = $"Bakiye yükleme - {paymentResult.TransactionId}",
                    TransactionDate = DateTime.UtcNow,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.WalletTransactions.AddAsync(walletTransaction);
                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();

                var result = new TopUpResultDto
                {
                    NewBalance = wallet.Balance,
                    TransactionId = walletTransaction.Id
                };

                return Response<TopUpResultDto>.Success(result, 200);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Response<WalletTransactionDto>> DeductAsync(string userId, decimal amount, ReferenceType referenceType, int? referenceId, string? description = null)
        {
            var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
            if (wallet == null)
                return Response<WalletTransactionDto>.Fail("Cüzdan bulunamadı", 404);

            if (!wallet.IsActive)
                return Response<WalletTransactionDto>.Fail("Cüzdan aktif değil", 400);

            if (wallet.Balance < amount)
                return Response<WalletTransactionDto>.Fail("Yetersiz bakiye", 400);

            // ACID Transaction başlat
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                wallet.Balance -= amount;
                wallet.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.Wallets.Update(wallet);

                var walletTransaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Type = TransactionType.Debit,
                    Amount = amount,
                    BalanceAfter = wallet.Balance,
                    ReferenceType = referenceType,
                    ReferenceId = referenceId,
                    Description = description ?? $"{referenceType} ödemesi",
                    TransactionDate = DateTime.UtcNow,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.WalletTransactions.AddAsync(walletTransaction);
                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();

                var dto = new WalletTransactionDto
                {
                    Id = walletTransaction.Id,
                    WalletId = walletTransaction.WalletId,
                    Type = walletTransaction.Type,
                    Amount = walletTransaction.Amount,
                    BalanceAfter = walletTransaction.BalanceAfter,
                    ReferenceType = walletTransaction.ReferenceType,
                    ReferenceId = walletTransaction.ReferenceId,
                    Description = walletTransaction.Description,
                    TransactionDate = walletTransaction.TransactionDate
                };

                return Response<WalletTransactionDto>.Success(dto, 200);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Response<WalletTransactionDto>> RefundAsync(string userId, decimal amount, ReferenceType referenceType, int? referenceId, string? description = null)
        {
            var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
            if (wallet == null)
                return Response<WalletTransactionDto>.Fail("Cüzdan bulunamadı", 404);

            // ACID Transaction başlat
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                wallet.Balance += amount;
                wallet.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.Wallets.Update(wallet);

                var walletTransaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Type = TransactionType.Credit,
                    Amount = amount,
                    BalanceAfter = wallet.Balance,
                    ReferenceType = ReferenceType.Refund,
                    ReferenceId = referenceId,
                    Description = description ?? $"{referenceType} iadesi",
                    TransactionDate = DateTime.UtcNow,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.WalletTransactions.AddAsync(walletTransaction);
                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();

                var dto = new WalletTransactionDto
                {
                    Id = walletTransaction.Id,
                    WalletId = walletTransaction.WalletId,
                    Type = walletTransaction.Type,
                    Amount = walletTransaction.Amount,
                    BalanceAfter = walletTransaction.BalanceAfter,
                    ReferenceType = walletTransaction.ReferenceType,
                    ReferenceId = walletTransaction.ReferenceId,
                    Description = walletTransaction.Description,
                    TransactionDate = walletTransaction.TransactionDate
                };

                return Response<WalletTransactionDto>.Success(dto, 200);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Response<WalletDto>> GetWalletByUserIdAsync(string userId)
        {
            return await GetWalletAsync(userId);
        }

        public async Task<Response<NoDataDto>> SetWalletStatusAsync(string userId, bool isActive)
        {
            var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
            if (wallet == null)
                return Response<NoDataDto>.Fail("Cüzdan bulunamadı", 404);

            wallet.IsActive = isActive;
            wallet.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.Wallets.Update(wallet);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        private async Task<Wallet> GetOrCreateWalletAsync(string userId)
        {
            var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);

            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = userId,
                    Balance = 0,
                    Currency = "TRY",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Wallets.AddAsync(wallet);
                await _unitOfWork.CommitAsync();
            }

            return wallet;
        }
    }
}
