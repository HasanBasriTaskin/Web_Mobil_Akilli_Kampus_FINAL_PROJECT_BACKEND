using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Wallet;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class WalletManager : IWalletService
    {
        private readonly CampusContext _context;
        private readonly IMockPaymentService _paymentService;

        public WalletManager(CampusContext context, IMockPaymentService paymentService)
        {
            _context = context;
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
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
                return Response<PagedResponse<WalletTransactionDto>>.Fail("Cüzdan bulunamadı", 404);

            var query = _context.WalletTransactions
                .Where(t => t.WalletId == wallet.Id)
                .OrderByDescending(t => t.TransactionDate);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var transactions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new WalletTransactionDto
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
                })
                .ToListAsync();

            var pagedResponse = new PagedResponse<WalletTransactionDto>(transactions, page, pageSize, totalCount);

            return Response<PagedResponse<WalletTransactionDto>>.Success(pagedResponse, 200);
        }

        public async Task<Response<TopUpResultDto>> TopUpAsync(string userId, WalletTopUpDto dto)
        {
            // WalletTopUpDto'yu PaymentDto'ya dönüştür
            var paymentDto = new PaymentDto
            {
                CardNumber = dto.CardNumber,
                CVV = dto.CVV,
                ExpiryDate = dto.ExpiryDate,
                Amount = dto.Amount
            };

            // Ödeme işlemini gerçekleştir
            var paymentResult = _paymentService.ProcessPayment(paymentDto);

            if (!paymentResult.IsSuccess)
            {
                return Response<TopUpResultDto>.Fail($"Ödeme başarısız: {paymentResult.ErrorMessage}", 400);
            }

            // Cüzdanı getir veya oluştur
            var wallet = await GetOrCreateWalletAsync(userId);

            if (!wallet.IsActive)
                return Response<TopUpResultDto>.Fail("Cüzdan aktif değil", 400);

            // Bakiyeyi güncelle
            wallet.Balance += paymentDto.Amount;
            wallet.UpdatedDate = DateTime.UtcNow;

            // İşlem kaydı oluştur
            var transaction = new WalletTransaction
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

            await _context.WalletTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            var result = new TopUpResultDto
            {
                NewBalance = wallet.Balance,
                TransactionId = transaction.Id
            };

            return Response<TopUpResultDto>.Success(result, 200);
        }

        public async Task<Response<WalletTransactionDto>> DeductAsync(string userId, decimal amount, ReferenceType referenceType, int? referenceId, string? description = null)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
                return Response<WalletTransactionDto>.Fail("Cüzdan bulunamadı", 404);

            if (!wallet.IsActive)
                return Response<WalletTransactionDto>.Fail("Cüzdan aktif değil", 400);

            if (wallet.Balance < amount)
                return Response<WalletTransactionDto>.Fail("Yetersiz bakiye", 400);

            // Bakiyeyi düş
            wallet.Balance -= amount;
            wallet.UpdatedDate = DateTime.UtcNow;

            // İşlem kaydı oluştur
            var transaction = new WalletTransaction
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

            await _context.WalletTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            var dto = new WalletTransactionDto
            {
                Id = transaction.Id,
                WalletId = transaction.WalletId,
                Type = transaction.Type,
                Amount = transaction.Amount,
                BalanceAfter = transaction.BalanceAfter,
                ReferenceType = transaction.ReferenceType,
                ReferenceId = transaction.ReferenceId,
                Description = transaction.Description,
                TransactionDate = transaction.TransactionDate
            };

            return Response<WalletTransactionDto>.Success(dto, 200);
        }

        public async Task<Response<WalletTransactionDto>> RefundAsync(string userId, decimal amount, ReferenceType referenceType, int? referenceId, string? description = null)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
                return Response<WalletTransactionDto>.Fail("Cüzdan bulunamadı", 404);

            // Bakiyeyi ekle (iade)
            wallet.Balance += amount;
            wallet.UpdatedDate = DateTime.UtcNow;

            // İşlem kaydı oluştur
            var transaction = new WalletTransaction
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

            await _context.WalletTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            var dto = new WalletTransactionDto
            {
                Id = transaction.Id,
                WalletId = transaction.WalletId,
                Type = transaction.Type,
                Amount = transaction.Amount,
                BalanceAfter = transaction.BalanceAfter,
                ReferenceType = transaction.ReferenceType,
                ReferenceId = transaction.ReferenceId,
                Description = transaction.Description,
                TransactionDate = transaction.TransactionDate
            };

            return Response<WalletTransactionDto>.Success(dto, 200);
        }

        public async Task<Response<WalletDto>> GetWalletByUserIdAsync(string userId)
        {
            return await GetWalletAsync(userId);
        }

        public async Task<Response<NoDataDto>> SetWalletStatusAsync(string userId, bool isActive)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
                return Response<NoDataDto>.Fail("Cüzdan bulunamadı", 404);

            wallet.IsActive = isActive;
            wallet.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        private async Task<Wallet> GetOrCreateWalletAsync(string userId)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);

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

                await _context.Wallets.AddAsync(wallet);
                await _context.SaveChangesAsync();
            }

            return wallet;
        }
    }
}
