using FluentAssertions;
using SMARTCAMPUS.BusinessLayer.Constants;
using Xunit;

namespace SMARTCAMPUS.Tests.Constants
{
    public class ConstantsTests
    {
        #region CampusNetworkConstants Tests

        [Fact]
        public void CampusIpRanges_ShouldNotBeEmpty()
        {
            // Assert
            CampusNetworkConstants.CampusIpRanges.Should().NotBeNullOrEmpty();
            CampusNetworkConstants.CampusIpRanges.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void CampusIpRanges_ShouldContainValidCidrRanges()
        {
            // Assert - each range should have valid CIDR notation
            foreach (var range in CampusNetworkConstants.CampusIpRanges)
            {
                range.Should().Contain("/");
                var parts = range.Split('/');
                parts.Length.Should().Be(2);
                int.TryParse(parts[1], out var prefix).Should().BeTrue();
                prefix.Should().BeInRange(0, 32);
            }
        }

        [Fact]
        public void MaxRealisticVelocity_ShouldBeReasonable()
        {
            // Assert
            CampusNetworkConstants.MaxRealisticVelocity.Should().Be(200m);
            CampusNetworkConstants.MaxRealisticVelocity.Should().BeGreaterThan(0);
        }

        [Fact]
        public void MaxTimeDifferenceForVelocityCheck_ShouldBe3600()
        {
            // Assert - 1 hour in seconds
            CampusNetworkConstants.MaxTimeDifferenceForVelocityCheck.Should().Be(3600);
        }

        [Fact]
        public void FraudScoreThresholds_ShouldBeInOrder()
        {
            // Assert
            CampusNetworkConstants.FraudScoreLow.Should().Be(30);
            CampusNetworkConstants.FraudScoreMedium.Should().Be(60);
            CampusNetworkConstants.FraudScoreHigh.Should().Be(80);

            CampusNetworkConstants.FraudScoreLow.Should().BeLessThan(CampusNetworkConstants.FraudScoreMedium);
            CampusNetworkConstants.FraudScoreMedium.Should().BeLessThan(CampusNetworkConstants.FraudScoreHigh);
        }

        #endregion


    }
}
