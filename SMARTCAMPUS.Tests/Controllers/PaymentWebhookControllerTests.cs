using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Wallet;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class PaymentWebhookControllerTests
    {
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly Mock<IWalletService> _mockWalletService;
        private readonly Mock<ILogger<PaymentWebhookController>> _mockLogger;
        private readonly PaymentWebhookController _controller;

        public PaymentWebhookControllerTests()
        {
            _mockPaymentService = new Mock<IPaymentService>();
            _mockWalletService = new Mock<IWalletService>();
            _mockLogger = new Mock<ILogger<PaymentWebhookController>>();
            _controller = new PaymentWebhookController(_mockPaymentService.Object, _mockWalletService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ReceiveCallback_ShouldRedirectToSuccess_WhenPaymentVerified()
        {
            var verificationResult = new PaymentVerificationResultDto
            {
                IsSuccess = true,
                PaidPrice = 100m,
                UserId = "user1",
                TransactionId = "TXN-123"
            };
            _mockPaymentService.Setup(x => x.VerifyPaymentAsync("token", "convId"))
                .ReturnsAsync(Response<PaymentVerificationResultDto>.Success(verificationResult, 200));
            _mockWalletService.Setup(x => x.AddBalanceAsync("user1", 100m, "TXN-123"))
                .ReturnsAsync(Response<TopUpResultDto>.Success(new TopUpResultDto { NewBalance = 100m, TransactionId = 1 }, 200));

            var result = await _controller.ReceiveCallback("token", "convId");

            result.Should().BeOfType<RedirectResult>();
            ((RedirectResult)result).Url.Should().Contain("payment/success");
        }

        [Fact]
        public async Task ReceiveCallback_ShouldRedirectToFail_WhenVerificationFails()
        {
            _mockPaymentService.Setup(x => x.VerifyPaymentAsync("token", "convId"))
                .ReturnsAsync(Response<PaymentVerificationResultDto>.Fail("Verification failed", 400));

            var result = await _controller.ReceiveCallback("token", "convId");

            result.Should().BeOfType<RedirectResult>();
            ((RedirectResult)result).Url.Should().Contain("payment/fail");
        }

        [Fact]
        public async Task ReceiveCallback_ShouldNotAddBalance_WhenUserIdIsEmpty()
        {
            var verificationResult = new PaymentVerificationResultDto
            {
                IsSuccess = true,
                PaidPrice = 100m,
                UserId = "",
                TransactionId = "TXN-123"
            };
            _mockPaymentService.Setup(x => x.VerifyPaymentAsync("token", "convId"))
                .ReturnsAsync(Response<PaymentVerificationResultDto>.Success(verificationResult, 200));

            var result = await _controller.ReceiveCallback("token", "convId");

            result.Should().BeOfType<RedirectResult>();
            _mockWalletService.Verify(x => x.AddBalanceAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
        }
    }
}

