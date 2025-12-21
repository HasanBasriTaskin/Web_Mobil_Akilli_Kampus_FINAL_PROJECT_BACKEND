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

        #region GradeConstants Tests

        [Fact]
        public void GradeWeights_ShouldSumTo100Percent()
        {
            // Assert
            var totalWeight = GradeConstants.MidtermWeight + GradeConstants.FinalWeight;
            totalWeight.Should().Be(1.0m);
        }

        [Fact]
        public void MidtermWeight_ShouldBe40Percent()
        {
            // Assert
            GradeConstants.MidtermWeight.Should().Be(0.4m);
        }

        [Fact]
        public void FinalWeight_ShouldBe60Percent()
        {
            // Assert
            GradeConstants.FinalWeight.Should().Be(0.6m);
        }

        [Fact]
        public void LateCheckInGracePeriodMinutes_ShouldBe15()
        {
            // Assert
            GradeConstants.LateCheckInGracePeriodMinutes.Should().Be(15);
        }

        [Fact]
        public void GradeThresholds_ShouldBeInDescendingOrder()
        {
            // Assert
            GradeConstants.GradeA.Should().Be(90);
            GradeConstants.GradeAMinus.Should().Be(85);
            GradeConstants.GradeBPlus.Should().Be(80);
            GradeConstants.GradeB.Should().Be(75);
            GradeConstants.GradeBMinus.Should().Be(70);
            GradeConstants.GradeCPlus.Should().Be(65);
            GradeConstants.GradeC.Should().Be(60);
            GradeConstants.GradeCMinus.Should().Be(55);
            GradeConstants.GradeD.Should().Be(50);

            // Verify descending order
            GradeConstants.GradeA.Should().BeGreaterThan(GradeConstants.GradeAMinus);
            GradeConstants.GradeAMinus.Should().BeGreaterThan(GradeConstants.GradeBPlus);
            GradeConstants.GradeBPlus.Should().BeGreaterThan(GradeConstants.GradeB);
            GradeConstants.GradeB.Should().BeGreaterThan(GradeConstants.GradeBMinus);
            GradeConstants.GradeBMinus.Should().BeGreaterThan(GradeConstants.GradeCPlus);
            GradeConstants.GradeCPlus.Should().BeGreaterThan(GradeConstants.GradeC);
            GradeConstants.GradeC.Should().BeGreaterThan(GradeConstants.GradeCMinus);
            GradeConstants.GradeCMinus.Should().BeGreaterThan(GradeConstants.GradeD);
        }

        #endregion

        #region GradePoints Tests

        [Fact]
        public void GradePoints_A_ShouldBe4Point0()
        {
            GradePoints.A.Should().Be(4.0m);
        }

        [Fact]
        public void GradePoints_AMinus_ShouldBe3Point7()
        {
            GradePoints.AMinus.Should().Be(3.7m);
        }

        [Fact]
        public void GradePoints_BPlus_ShouldBe3Point3()
        {
            GradePoints.BPlus.Should().Be(3.3m);
        }

        [Fact]
        public void GradePoints_B_ShouldBe3Point0()
        {
            GradePoints.B.Should().Be(3.0m);
        }

        [Fact]
        public void GradePoints_BMinus_ShouldBe2Point7()
        {
            GradePoints.BMinus.Should().Be(2.7m);
        }

        [Fact]
        public void GradePoints_CPlus_ShouldBe2Point3()
        {
            GradePoints.CPlus.Should().Be(2.3m);
        }

        [Fact]
        public void GradePoints_C_ShouldBe2Point0()
        {
            GradePoints.C.Should().Be(2.0m);
        }

        [Fact]
        public void GradePoints_CMinus_ShouldBe1Point7()
        {
            GradePoints.CMinus.Should().Be(1.7m);
        }

        [Fact]
        public void GradePoints_D_ShouldBe1Point0()
        {
            GradePoints.D.Should().Be(1.0m);
        }

        [Fact]
        public void GradePoints_F_ShouldBe0()
        {
            GradePoints.F.Should().Be(0.0m);
        }

        [Fact]
        public void GradePoints_ShouldBeInDescendingOrder()
        {
            // Verify all grade points are in proper descending order
            GradePoints.A.Should().BeGreaterThan(GradePoints.AMinus);
            GradePoints.AMinus.Should().BeGreaterThan(GradePoints.BPlus);
            GradePoints.BPlus.Should().BeGreaterThan(GradePoints.B);
            GradePoints.B.Should().BeGreaterThan(GradePoints.BMinus);
            GradePoints.BMinus.Should().BeGreaterThan(GradePoints.CPlus);
            GradePoints.CPlus.Should().BeGreaterThan(GradePoints.C);
            GradePoints.C.Should().BeGreaterThan(GradePoints.CMinus);
            GradePoints.CMinus.Should().BeGreaterThan(GradePoints.D);
            GradePoints.D.Should().BeGreaterThan(GradePoints.F);
        }

        #endregion
    }
}
