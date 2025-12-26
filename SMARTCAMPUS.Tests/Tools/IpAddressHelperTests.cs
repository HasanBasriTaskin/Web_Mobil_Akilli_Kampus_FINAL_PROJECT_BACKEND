using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SMARTCAMPUS.BusinessLayer.Tools;
using Xunit;

namespace SMARTCAMPUS.Tests.Tools
{
    public class IpAddressHelperTests
    {
        #region IsCampusIpAddress Tests

        [Fact]
        public void IsCampusIpAddress_ShouldReturnFalse_WhenNullOrEmpty()
        {
            // Act & Assert
            IpAddressHelper.IsCampusIpAddress(null).Should().BeFalse();
            IpAddressHelper.IsCampusIpAddress("").Should().BeFalse();
            IpAddressHelper.IsCampusIpAddress("   ").Should().BeFalse();
        }

        [Fact]
        public void IsCampusIpAddress_ShouldReturnFalse_WhenInvalidIpAddress()
        {
            // Act & Assert
            IpAddressHelper.IsCampusIpAddress("invalid").Should().BeFalse();
            IpAddressHelper.IsCampusIpAddress("256.256.256.256").Should().BeFalse();
            IpAddressHelper.IsCampusIpAddress("not.an.ip.address").Should().BeFalse();
        }

        [Fact]
        public void IsCampusIpAddress_ShouldReturnTrue_WhenInCampusRange_192_168_1()
        {
            // Act & Assert - These IPs should be in the 192.168.1.0/24 range
            IpAddressHelper.IsCampusIpAddress("192.168.1.1").Should().BeTrue();
            IpAddressHelper.IsCampusIpAddress("192.168.1.100").Should().BeTrue();
            IpAddressHelper.IsCampusIpAddress("192.168.1.255").Should().BeTrue();
        }

        [Fact]
        public void IsCampusIpAddress_ShouldReturnTrue_WhenInCampusRange_192_168_2()
        {
            // Act & Assert - These IPs should be in the 192.168.2.0/24 range
            IpAddressHelper.IsCampusIpAddress("192.168.2.1").Should().BeTrue();
            IpAddressHelper.IsCampusIpAddress("192.168.2.50").Should().BeTrue();
        }

        [Fact]
        public void IsCampusIpAddress_ShouldReturnTrue_WhenInCampusRange_10_0()
        {
            // Act & Assert - These IPs should be in the 10.0.0.0/16 range
            IpAddressHelper.IsCampusIpAddress("10.0.0.1").Should().BeTrue();
            IpAddressHelper.IsCampusIpAddress("10.0.255.255").Should().BeTrue();
        }

        [Fact]
        public void IsCampusIpAddress_ShouldReturnTrue_WhenInVpnRange_172_16()
        {
            // Act & Assert - These IPs should be in the 172.16.0.0/12 range
            IpAddressHelper.IsCampusIpAddress("172.16.0.1").Should().BeTrue();
            IpAddressHelper.IsCampusIpAddress("172.31.255.255").Should().BeTrue();
        }

        [Fact]
        public void IsCampusIpAddress_ShouldReturnFalse_WhenOutsideCampusRanges()
        {
            // Act & Assert - These IPs should be outside all campus ranges
            IpAddressHelper.IsCampusIpAddress("8.8.8.8").Should().BeFalse();
            IpAddressHelper.IsCampusIpAddress("192.168.3.1").Should().BeFalse(); // Not in 192.168.1.0/24 or 192.168.2.0/24
            IpAddressHelper.IsCampusIpAddress("172.32.0.1").Should().BeFalse(); // Outside 172.16.0.0/12
            IpAddressHelper.IsCampusIpAddress("10.1.0.1").Should().BeFalse(); // Outside 10.0.0.0/16
        }

        #endregion

        #region GetClientIpAddress Tests

        [Fact]
        public void GetClientIpAddress_ShouldReturnNull_WhenHttpContextIsNull()
        {
            // Act
            var result = IpAddressHelper.GetClientIpAddress(null);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetClientIpAddress_ShouldReturnForwardedFor_WhenHeaderExists()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Forwarded-For"] = "192.168.1.100, 10.0.0.1";

            // Act
            var result = IpAddressHelper.GetClientIpAddress(context);

            // Assert
            result.Should().Be("192.168.1.100");
        }

        [Fact]
        public void GetClientIpAddress_ShouldReturnForwardedFor_WhenSingleIp()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Forwarded-For"] = "192.168.1.50";

            // Act
            var result = IpAddressHelper.GetClientIpAddress(context);

            // Assert
            result.Should().Be("192.168.1.50");
        }

        [Fact]
        public void GetClientIpAddress_ShouldReturnRealIp_WhenXRealIpHeaderExists()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Real-IP"] = "10.0.0.50";

            // Act
            var result = IpAddressHelper.GetClientIpAddress(context);

            // Assert
            result.Should().Be("10.0.0.50");
        }

        [Fact]
        public void GetClientIpAddress_ShouldPreferForwardedFor_OverRealIp()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Forwarded-For"] = "192.168.1.1";
            context.Request.Headers["X-Real-IP"] = "10.0.0.1";

            // Act
            var result = IpAddressHelper.GetClientIpAddress(context);

            // Assert
            result.Should().Be("192.168.1.1");
        }

        [Fact]
        public void GetClientIpAddress_ShouldIgnoreEmptyForwardedFor()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Forwarded-For"] = "";
            context.Request.Headers["X-Real-IP"] = "10.0.0.50";

            // Act
            var result = IpAddressHelper.GetClientIpAddress(context);

            // Assert
            result.Should().Be("10.0.0.50");
        }

        [Fact]
        public void GetClientIpAddress_ShouldReturnNull_WhenNoHeadersAndNoRemoteIpAddress()
        {
            // Arrange
            var context = new DefaultHttpContext();
            // No headers set and no remote IP

            // Act
            var result = IpAddressHelper.GetClientIpAddress(context);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetClientIpAddress_ShouldTrimWhitespace_FromForwardedFor()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Forwarded-For"] = "  192.168.1.100  , 10.0.0.1";

            // Act
            var result = IpAddressHelper.GetClientIpAddress(context);

            // Assert
            result.Should().Be("192.168.1.100");
        }

        #endregion
    }
}
