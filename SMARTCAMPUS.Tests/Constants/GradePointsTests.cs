using FluentAssertions;
using SMARTCAMPUS.BusinessLayer.Constants;
using Xunit;

namespace SMARTCAMPUS.Tests.Constants
{
    public class GradePointsTests
    {
        [Fact]
        public void GradePoints_ShouldHaveCorrectValues()
        {
            GradePoints.A.Should().Be(4.0m);
            GradePoints.AMinus.Should().Be(3.7m);
            GradePoints.BPlus.Should().Be(3.3m);
            GradePoints.B.Should().Be(3.0m);
            GradePoints.BMinus.Should().Be(2.7m);
            GradePoints.CPlus.Should().Be(2.3m);
            GradePoints.C.Should().Be(2.0m);
            GradePoints.CMinus.Should().Be(1.7m);
            GradePoints.D.Should().Be(1.0m);
            GradePoints.F.Should().Be(0.0m);
        }

        [Fact]
        public void GradePoints_ShouldBeInDescendingOrder()
        {
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

        [Fact]
        public void GradePoints_ShouldBeInValidRange()
        {
            GradePoints.A.Should().BeInRange(0, 4);
            GradePoints.AMinus.Should().BeInRange(0, 4);
            GradePoints.BPlus.Should().BeInRange(0, 4);
            GradePoints.B.Should().BeInRange(0, 4);
            GradePoints.BMinus.Should().BeInRange(0, 4);
            GradePoints.CPlus.Should().BeInRange(0, 4);
            GradePoints.C.Should().BeInRange(0, 4);
            GradePoints.CMinus.Should().BeInRange(0, 4);
            GradePoints.D.Should().BeInRange(0, 4);
            GradePoints.F.Should().BeInRange(0, 4);
        }

        [Fact]
        public void GradePoints_ShouldNotBeNegative()
        {
            GradePoints.A.Should().BeGreaterThanOrEqualTo(0);
            GradePoints.AMinus.Should().BeGreaterThanOrEqualTo(0);
            GradePoints.BPlus.Should().BeGreaterThanOrEqualTo(0);
            GradePoints.B.Should().BeGreaterThanOrEqualTo(0);
            GradePoints.BMinus.Should().BeGreaterThanOrEqualTo(0);
            GradePoints.CPlus.Should().BeGreaterThanOrEqualTo(0);
            GradePoints.C.Should().BeGreaterThanOrEqualTo(0);
            GradePoints.CMinus.Should().BeGreaterThanOrEqualTo(0);
            GradePoints.D.Should().BeGreaterThanOrEqualTo(0);
            GradePoints.F.Should().BeGreaterThanOrEqualTo(0);
        }
    }
}

