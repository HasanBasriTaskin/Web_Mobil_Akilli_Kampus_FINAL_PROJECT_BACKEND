using FluentAssertions;
using SMARTCAMPUS.BusinessLayer.Constants;
using Xunit;

namespace SMARTCAMPUS.Tests.Constants
{
    public class GradeConstantsTests
    {
        [Fact]
        public void GradeWeights_ShouldSumTo100Percent()
        {
            var totalWeight = GradeConstants.MidtermWeight + GradeConstants.FinalWeight;
            totalWeight.Should().Be(1.0m);
        }

        [Fact]
        public void MidtermWeight_ShouldBe40Percent()
        {
            GradeConstants.MidtermWeight.Should().Be(0.4m);
        }

        [Fact]
        public void FinalWeight_ShouldBe60Percent()
        {
            GradeConstants.FinalWeight.Should().Be(0.6m);
        }

        [Fact]
        public void LateCheckInGracePeriodMinutes_ShouldBe15()
        {
            GradeConstants.LateCheckInGracePeriodMinutes.Should().Be(15);
        }

        [Fact]
        public void GradeThresholds_ShouldHaveCorrectValues()
        {
            GradeConstants.GradeA.Should().Be(90);
            GradeConstants.GradeAMinus.Should().Be(85);
            GradeConstants.GradeBPlus.Should().Be(80);
            GradeConstants.GradeB.Should().Be(75);
            GradeConstants.GradeBMinus.Should().Be(70);
            GradeConstants.GradeCPlus.Should().Be(65);
            GradeConstants.GradeC.Should().Be(60);
            GradeConstants.GradeCMinus.Should().Be(55);
            GradeConstants.GradeD.Should().Be(50);
        }

        [Fact]
        public void GradeThresholds_ShouldBeInDescendingOrder()
        {
            GradeConstants.GradeA.Should().BeGreaterThan(GradeConstants.GradeAMinus);
            GradeConstants.GradeAMinus.Should().BeGreaterThan(GradeConstants.GradeBPlus);
            GradeConstants.GradeBPlus.Should().BeGreaterThan(GradeConstants.GradeB);
            GradeConstants.GradeB.Should().BeGreaterThan(GradeConstants.GradeBMinus);
            GradeConstants.GradeBMinus.Should().BeGreaterThan(GradeConstants.GradeCPlus);
            GradeConstants.GradeCPlus.Should().BeGreaterThan(GradeConstants.GradeC);
            GradeConstants.GradeC.Should().BeGreaterThan(GradeConstants.GradeCMinus);
            GradeConstants.GradeCMinus.Should().BeGreaterThan(GradeConstants.GradeD);
        }

        [Fact]
        public void GradeThresholds_ShouldBeInValidRange()
        {
            GradeConstants.GradeA.Should().BeInRange(0, 100);
            GradeConstants.GradeAMinus.Should().BeInRange(0, 100);
            GradeConstants.GradeBPlus.Should().BeInRange(0, 100);
            GradeConstants.GradeB.Should().BeInRange(0, 100);
            GradeConstants.GradeBMinus.Should().BeInRange(0, 100);
            GradeConstants.GradeCPlus.Should().BeInRange(0, 100);
            GradeConstants.GradeC.Should().BeInRange(0, 100);
            GradeConstants.GradeCMinus.Should().BeInRange(0, 100);
            GradeConstants.GradeD.Should().BeInRange(0, 100);
        }

        [Fact]
        public void GradeWeights_ShouldBeInValidRange()
        {
            GradeConstants.MidtermWeight.Should().BeGreaterThan(0);
            GradeConstants.MidtermWeight.Should().BeLessThanOrEqualTo(1);
            GradeConstants.FinalWeight.Should().BeGreaterThan(0);
            GradeConstants.FinalWeight.Should().BeLessThanOrEqualTo(1);
        }
    }
}

