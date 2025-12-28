using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Wallet;
using SMARTCAMPUS.EntityLayer.Enums;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class WalletControllerTests
    {
        private readonly Mock<IWalletService> _mockWalletService;
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly WalletController _walletController;

        public WalletControllerTests()
        {
            _mockWalletService = new Mock<IWalletService>();
            _mockPaymentService = new Mock<IPaymentService>();
            _walletController = new WalletController(_mockWalletService.Object);
        }

        private void SetupHttpContext(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _walletController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        #region GetWallet Tests

        [Fact]
        public async Task GetWallet_WithValidUser_ShouldReturnWallet()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var walletDto = new WalletDto
            {
                Id = 1,
                UserId = userId,
                Balance = 100.00m,
                Currency = "TRY",
                IsActive = true
            };

            var response = Response<WalletDto>.Success(walletDto, 200);
            _mockWalletService.Setup(s => s.GetWalletAsync(userId)).ReturnsAsync(response);

            // Act
            var result = await _walletController.GetWallet();

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task GetWallet_WithUnauthorizedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            _walletController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = await _walletController.GetWallet();

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        #endregion

        #region GetTransactions Tests

        [Fact]
        public async Task GetTransactions_WithValidUser_ShouldReturnTransactions()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var transactions = new List<WalletTransactionDto>
            {
                new WalletTransactionDto
                {
                    Id = 1,
                    Type = TransactionType.Credit,
                    Amount = 100.00m,
                    BalanceAfter = 100.00m,
                    Description = "Top-up",
                    TransactionDate = DateTime.UtcNow
                }
            };

            var pagedResponse = new PagedResponse<WalletTransactionDto>(transactions, 1, 20, 1);

            var response = Response<PagedResponse<WalletTransactionDto>>.Success(pagedResponse, 200);
            _mockWalletService.Setup(s => s.GetTransactionsAsync(userId, 1, 20)).ReturnsAsync(response);

            // Act
            var result = await _walletController.GetTransactions(1, 20);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task GetTransactions_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var pagedResponse = new PagedResponse<WalletTransactionDto>(new List<WalletTransactionDto>(), 2, 10, 0);

            var response = Response<PagedResponse<WalletTransactionDto>>.Success(pagedResponse, 200);
            _mockWalletService.Setup(s => s.GetTransactionsAsync(userId, 2, 10)).ReturnsAsync(response);

            // Act
            var result = await _walletController.GetTransactions(2, 10);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        #endregion

        #region TopUp Tests

        [Fact]
        public async Task TopUp_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var topUpDto = new WalletTopUpDto
            {
                Amount = 100.00m,
                CardNumber = "1234-5678-1234-5678",
                CVV = "123",
                ExpiryDate = "01/26"
            };

            var topUpResult = new TopUpResultDto
            {
                TransactionId = 123456,
                NewBalance = 200.00m
            };

            var response = Response<TopUpResultDto>.Success(topUpResult, 200);
            _mockWalletService.Setup(s => s.TopUpAsync(userId, topUpDto)).ReturnsAsync(response);

            // Act
            var result = await _walletController.TopUp(topUpDto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task TopUp_WithInvalidAmount_ShouldReturnError()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var topUpDto = new WalletTopUpDto
            {
                Amount = 0m, // Invalid amount
                CardNumber = "1234-5678-1234-5678",
                CVV = "123",
                ExpiryDate = "01/26"
            };

            var response = Response<TopUpResultDto>.Fail("Geçersiz tutar", 400);
            _mockWalletService.Setup(s => s.TopUpAsync(userId, topUpDto)).ReturnsAsync(response);

            // Act
            var result = await _walletController.TopUp(topUpDto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task TopUp_WithUnauthorizedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            _walletController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            var topUpDto = new WalletTopUpDto
            {
                Amount = 100.00m,
                CardNumber = "1234-5678-1234-5678",
                CVV = "123",
                ExpiryDate = "01/26"
            };

            // Act
            var result = await _walletController.TopUp(topUpDto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        #endregion

        #region GetUserWallet (Admin) Tests

        [Fact]
        public async Task GetUserWallet_WithAdminRole_ShouldReturnWallet()
        {
            // Arrange
            var adminUserId = "admin1";
            var targetUserId = "user1";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, adminUserId),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _walletController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var walletDto = new WalletDto
            {
                Id = 1,
                UserId = targetUserId,
                Balance = 150.00m,
                Currency = "TRY",
                IsActive = true
            };

            var response = Response<WalletDto>.Success(walletDto, 200);
            _mockWalletService.Setup(s => s.GetWalletByUserIdAsync(targetUserId)).ReturnsAsync(response);

            // Act
            var result = _walletController.GetUserWallet(targetUserId);

            // Assert
            var task = Assert.IsType<Task<IActionResult>>(result);
            var objectResult = task.Result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        #endregion

        #region SetStatus (Admin) Tests

        [Fact]
        public async Task SetStatus_WithAdminRole_ShouldUpdateStatus()
        {
            // Arrange
            var adminUserId = "admin1";
            var targetUserId = "user1";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, adminUserId),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _walletController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var response = Response<NoDataDto>.Success(200);
            _mockWalletService.Setup(s => s.SetWalletStatusAsync(targetUserId, false)).ReturnsAsync(response);

            // Act
            var result = await _walletController.SetStatus(targetUserId, false);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        #endregion
    }
}

