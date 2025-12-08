using FluentAssertions;
using Moq;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class DepartmentManagerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IDepartmentDal> _mockDepartmentDal;
        private readonly DepartmentManager _departmentManager;

        public DepartmentManagerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockDepartmentDal = new Mock<IDepartmentDal>();

            _mockUnitOfWork.Setup(u => u.Departments).Returns(_mockDepartmentDal.Object);

            _departmentManager = new DepartmentManager(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetDepartmentsAsync_ShouldReturnSuccess_WithData()
        {
            // Arrange
            var departments = new List<Department>
            {
                new Department { Id = 1, Name = "D1" },
                new Department { Id = 2, Name = "D2" }
            };

            _mockDepartmentDal.Setup(x => x.GetAllAsync()).ReturnsAsync(departments);

            // Act
            var result = await _departmentManager.GetDepartmentsAsync();

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Data.Should().BeEquivalentTo(departments);
        }
    }
}
