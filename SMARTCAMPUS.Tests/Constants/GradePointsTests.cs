using FluentAssertions;
using SMARTCAMPUS.BusinessLayer.Constants;
using Xunit;

namespace SMARTCAMPUS.Tests.Constants
{
    public class GradePointsTests
    {
        [Fact]
        public void A_ShouldBe4Point0()
        {
            var point = GradePoints.A;
            point.Should().Be(4.0m);
        }

        [Fact]
        public void AMinus_ShouldBe3Point7()
        {
            var point = GradePoints.AMinus;
            point.Should().Be(3.7m);
        }

        [Fact]
        public void BPlus_ShouldBe3Point3()
        {
            var point = GradePoints.BPlus;
            point.Should().Be(3.3m);
        }

        [Fact]
        public void B_ShouldBe3Point0()
        {
            var point = GradePoints.B;
            point.Should().Be(3.0m);
        }

        [Fact]
        public void BMinus_ShouldBe2Point7()
        {
            var point = GradePoints.BMinus;
            point.Should().Be(2.7m);
        }

        [Fact]
        public void CPlus_ShouldBe2Point3()
        {
            var point = GradePoints.CPlus;
            point.Should().Be(2.3m);
        }

        [Fact]
        public void C_ShouldBe2Point0()
        {
            var point = GradePoints.C;
            point.Should().Be(2.0m);
        }

        [Fact]
        public void CMinus_ShouldBe1Point7()
        {
            var point = GradePoints.CMinus;
            point.Should().Be(1.7m);
        }

        [Fact]
        public void D_ShouldBe1Point0()
        {
            var point = GradePoints.D;
            point.Should().Be(1.0m);
        }

        [Fact]
        public void F_ShouldBe0Point0()
        {
            var point = GradePoints.F;
            point.Should().Be(0.0m);
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

