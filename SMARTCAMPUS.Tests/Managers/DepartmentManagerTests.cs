using AutoMapper;
using FluentAssertions;
using Moq;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class DepartmentManagerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IDepartmentDal> _mockDepartmentDal;
        private readonly Mock<IMapper> _mockMapper;
        private readonly DepartmentManager _departmentManager;

        public DepartmentManagerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockDepartmentDal = new Mock<IDepartmentDal>();
            _mockMapper = new Mock<IMapper>();

            _mockUnitOfWork.Setup(u => u.Departments).Returns(_mockDepartmentDal.Object);

            _departmentManager = new DepartmentManager(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetDepartmentsAsync_ShouldReturnSuccess_WithData()
        {
            // Arrange
            var departments = new List<Department>
            {
                new Department { Id = 1, Name = "D1", Code = "D1" },
                new Department { Id = 2, Name = "D2", Code = "D2" }
            };

            var departmentDtos = new List<DepartmentDto>
            {
                new DepartmentDto { Id = 1, Name = "D1", Code = "D1" },
                new DepartmentDto { Id = 2, Name = "D2", Code = "D2" }
            };

            _mockDepartmentDal.Setup(x => x.GetAllAsync()).ReturnsAsync(departments);
            _mockMapper.Setup(m => m.Map<List<DepartmentDto>>(departments)).Returns(departmentDtos);

            // Act
            var result = await _departmentManager.GetDepartmentsAsync();

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Data.Should().BeEquivalentTo(departmentDtos);
        }

        [Fact]
        public async Task GetDepartmentsAsync_ShouldReturnSuccess_WithEmptyList()
        {
            // Arrange
            var emptyList = new List<Department>();
            var emptyDtos = new List<DepartmentDto>();

            _mockDepartmentDal.Setup(x => x.GetAllAsync()).ReturnsAsync(emptyList);
            _mockMapper.Setup(m => m.Map<List<DepartmentDto>>(emptyList)).Returns(emptyDtos);

            // Act
            var result = await _departmentManager.GetDepartmentsAsync();

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDepartmentsAsync_ShouldReturnFail_OnException()
        {
            // Arrange
            _mockDepartmentDal.Setup(x => x.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _departmentManager.GetDepartmentsAsync();

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(500);
            result.Errors.Should().Contain(e => e.Contains("Database error"));
        }
    }
}
