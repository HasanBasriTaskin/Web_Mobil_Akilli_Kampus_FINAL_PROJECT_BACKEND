using FluentAssertions;
using Moq;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Wallet;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class WalletManagerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IWalletDal> _mockWalletDal;
        private readonly Mock<IWalletTransactionDal> _mockTransactionDal;
        private readonly Mock<IMockPaymentService> _mockPaymentService;
        private readonly WalletManager _manager;

        public WalletManagerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockWalletDal = new Mock<IWalletDal>();
            _mockTransactionDal = new Mock<IWalletTransactionDal>();
            _mockPaymentService = new Mock<IMockPaymentService>();
            _mockUnitOfWork.Setup(u => u.Wallets).Returns(_mockWalletDal.Object);
            _mockUnitOfWork.Setup(u => u.WalletTransactions).Returns(_mockTransactionDal.Object);
            _manager = new WalletManager(_mockUnitOfWork.Object, _mockPaymentService.Object);
        }

        [Fact]
        public async Task GetWalletAsync_ShouldReturnSuccess_WhenExists()
        {
            var wallet = new Wallet { Id = 1, UserId = "user1", Balance = 100m, IsActive = true };
            _mockWalletDal.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync(wallet);
            _mockWalletDal.Setup(x => x.AddAsync(It.IsAny<Wallet>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.GetWalletAsync("user1");

            result.IsSuccessful.Should().BeTrue();
            result.Data.Balance.Should().Be(100m);
        }

        [Fact]
        public async Task GetWalletAsync_ShouldCreateWallet_WhenNotExists()
        {
            _mockWalletDal.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync((Wallet)null!);
            _mockWalletDal.Setup(x => x.AddAsync(It.IsAny<Wallet>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.GetWalletAsync("user1");

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task TopUpAsync_ShouldReturnSuccess_WhenValid()
        {
            var wallet = new Wallet { Id = 1, UserId = "user1", Balance = 100m, IsActive = true };
            var dto = new WalletTopUpDto { CardNumber = "1234567890123456", CVV = "123", ExpiryDate = "12/25", Amount = 50m };
            var paymentResult = new PaymentResultDto { IsSuccess = true, TransactionId = "TXN123" };
            _mockPaymentService.Setup(x => x.ProcessPayment(It.IsAny<PaymentDto>())).Returns(paymentResult);
            _mockWalletDal.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync(wallet);
            _mockWalletDal.Setup(x => x.Update(It.IsAny<Wallet>()));
            _mockTransactionDal.Setup(x => x.AddAsync(It.IsAny<WalletTransaction>())).Returns(Task.CompletedTask);
            var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(mockTransaction.Object);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.TopUpAsync("user1", dto);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task TopUpAsync_ShouldReturnFail_WhenPaymentFails()
        {
            var dto = new WalletTopUpDto { CardNumber = "1234567890123456", CVV = "123", ExpiryDate = "12/25", Amount = 50m };
            var paymentResult = new PaymentResultDto { IsSuccess = false, ErrorMessage = "Payment failed" };
            _mockPaymentService.Setup(x => x.ProcessPayment(It.IsAny<PaymentDto>())).Returns(paymentResult);

            var result = await _manager.TopUpAsync("user1", dto);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task DeductAsync_ShouldReturnSuccess_WhenValid()
        {
            var wallet = new Wallet { Id = 1, UserId = "user1", Balance = 100m, IsActive = true };
            _mockWalletDal.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync(wallet);
            _mockWalletDal.Setup(x => x.Update(It.IsAny<Wallet>()));
            _mockTransactionDal.Setup(x => x.AddAsync(It.IsAny<WalletTransaction>())).Returns(Task.CompletedTask);
            var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(mockTransaction.Object);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.DeductAsync("user1", 50m, ReferenceType.MealReservation, 1);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task DeductAsync_ShouldReturnFail_WhenInsufficientBalance()
        {
            var wallet = new Wallet { Id = 1, UserId = "user1", Balance = 30m, IsActive = true };
            _mockWalletDal.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync(wallet);

            var result = await _manager.DeductAsync("user1", 50m, ReferenceType.MealReservation, 1);

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task RefundAsync_ShouldReturnSuccess_WhenValid()
        {
            var wallet = new Wallet { Id = 1, UserId = "user1", Balance = 100m, IsActive = true };
            _mockWalletDal.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync(wallet);
            _mockWalletDal.Setup(x => x.Update(It.IsAny<Wallet>()));
            _mockTransactionDal.Setup(x => x.AddAsync(It.IsAny<WalletTransaction>())).Returns(Task.CompletedTask);
            var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(mockTransaction.Object);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _manager.RefundAsync("user1", 50m, ReferenceType.MealReservation, 1);

            result.IsSuccessful.Should().BeTrue();
        }
    }
}

