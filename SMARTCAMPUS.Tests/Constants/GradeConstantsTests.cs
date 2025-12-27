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
            var weight = GradeConstants.MidtermWeight;
            weight.Should().Be(0.4m);
        }

        [Fact]
        public void FinalWeight_ShouldBe60Percent()
        {
            var weight = GradeConstants.FinalWeight;
            weight.Should().Be(0.6m);
        }

        [Fact]
        public void LateCheckInGracePeriodMinutes_ShouldBe15()
        {
            var minutes = GradeConstants.LateCheckInGracePeriodMinutes;
            minutes.Should().Be(15);
        }

        [Fact]
        public void GradeA_ShouldBe90()
        {
            var grade = GradeConstants.GradeA;
            grade.Should().Be(90);
        }

        [Fact]
        public void GradeAMinus_ShouldBe85()
        {
            var grade = GradeConstants.GradeAMinus;
            grade.Should().Be(85);
        }

        [Fact]
        public void GradeBPlus_ShouldBe80()
        {
            var grade = GradeConstants.GradeBPlus;
            grade.Should().Be(80);
        }

        [Fact]
        public void GradeB_ShouldBe75()
        {
            var grade = GradeConstants.GradeB;
            grade.Should().Be(75);
        }

        [Fact]
        public void GradeBMinus_ShouldBe70()
        {
            var grade = GradeConstants.GradeBMinus;
            grade.Should().Be(70);
        }

        [Fact]
        public void GradeCPlus_ShouldBe65()
        {
            var grade = GradeConstants.GradeCPlus;
            grade.Should().Be(65);
        }

        [Fact]
        public void GradeC_ShouldBe60()
        {
            var grade = GradeConstants.GradeC;
            grade.Should().Be(60);
        }

        [Fact]
        public void GradeCMinus_ShouldBe55()
        {
            var grade = GradeConstants.GradeCMinus;
            grade.Should().Be(55);
        }

        [Fact]
        public void GradeD_ShouldBe50()
        {
            var grade = GradeConstants.GradeD;
            grade.Should().Be(50);
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

        [Fact]
        public void LateCheckInGracePeriodMinutes_ShouldBePositive()
        {
            GradeConstants.LateCheckInGracePeriodMinutes.Should().BeGreaterThan(0);
        }

        [Fact]
        public void LateCheckInGracePeriodMinutes_ShouldBeReasonable()
        {
            GradeConstants.LateCheckInGracePeriodMinutes.Should().BeLessThanOrEqualTo(60);
        }

        [Fact]
        public void AllGradeConstants_ShouldBeAccessible()
        {
            // Test that all constants can be accessed without throwing exceptions
            var constants = new[]
            {
                GradeConstants.MidtermWeight,
                GradeConstants.FinalWeight,
                GradeConstants.LateCheckInGracePeriodMinutes,
                GradeConstants.GradeA,
                GradeConstants.GradeAMinus,
                GradeConstants.GradeBPlus,
                GradeConstants.GradeB,
                GradeConstants.GradeBMinus,
                GradeConstants.GradeCPlus,
                GradeConstants.GradeC,
                GradeConstants.GradeCMinus,
                GradeConstants.GradeD
            };

            constants.Should().NotBeEmpty();
            constants.Should().OnlyContain(c => c >= 0);
        }

        [Fact]
        public void GradeThresholds_ShouldHaveProperSpacing()
        {
            // Verify that grade thresholds have consistent spacing (5 points between each)
            (GradeConstants.GradeA - GradeConstants.GradeAMinus).Should().Be(5);
            (GradeConstants.GradeAMinus - GradeConstants.GradeBPlus).Should().Be(5);
            (GradeConstants.GradeBPlus - GradeConstants.GradeB).Should().Be(5);
            (GradeConstants.GradeB - GradeConstants.GradeBMinus).Should().Be(5);
            (GradeConstants.GradeBMinus - GradeConstants.GradeCPlus).Should().Be(5);
            (GradeConstants.GradeCPlus - GradeConstants.GradeC).Should().Be(5);
            (GradeConstants.GradeC - GradeConstants.GradeCMinus).Should().Be(5);
            (GradeConstants.GradeCMinus - GradeConstants.GradeD).Should().Be(5);
        }
    }
}

