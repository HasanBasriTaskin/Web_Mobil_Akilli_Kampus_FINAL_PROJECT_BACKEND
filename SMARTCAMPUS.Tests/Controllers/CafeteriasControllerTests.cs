using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Cafeteria;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class CafeteriasControllerTests
    {
        private readonly Mock<ICafeteriaService> _mockCafeteriaService;
        private readonly CafeteriasController _controller;

        public CafeteriasControllerTests()
        {
            _mockCafeteriaService = new Mock<ICafeteriaService>();
            _controller = new CafeteriasController(_mockCafeteriaService.Object);
        }

        private void SetupHttpContext(string? role = null)
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WhenAnonymous()
        {
            SetupHttpContext();
            _mockCafeteriaService.Setup(x => x.GetAllAsync(false))
                .ReturnsAsync(Response<List<CafeteriaDto>>.Success(new List<CafeteriaDto>(), 200));

            var result = await _controller.GetAll();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetAll_ShouldIncludeInactive_WhenAdmin()
        {
            SetupHttpContext("Admin");
            _mockCafeteriaService.Setup(x => x.GetAllAsync(true))
                .ReturnsAsync(Response<List<CafeteriaDto>>.Success(new List<CafeteriaDto>(), 200));

            var result = await _controller.GetAll(includeInactive: true);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk()
        {
            _mockCafeteriaService.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(Response<CafeteriaDto>.Success(new CafeteriaDto(), 200));

            var result = await _controller.GetById(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Create_ShouldReturnCreated()
        {
            SetupHttpContext("Admin");
            var dto = new CafeteriaCreateDto { Name = "Test Cafeteria", Location = "Building A" };
            _mockCafeteriaService.Setup(x => x.CreateAsync(dto))
                .ReturnsAsync(Response<CafeteriaDto>.Success(new CafeteriaDto(), 201));

            var result = await _controller.Create(dto);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task Update_ShouldReturnOk()
        {
            SetupHttpContext("Admin");
            var dto = new CafeteriaUpdateDto { Name = "Updated Cafeteria" };
            _mockCafeteriaService.Setup(x => x.UpdateAsync(1, dto))
                .ReturnsAsync(Response<CafeteriaDto>.Success(new CafeteriaDto(), 200));

            var result = await _controller.Update(1, dto);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Delete_ShouldReturnOk()
        {
            SetupHttpContext("Admin");
            _mockCafeteriaService.Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(Response<NoDataDto>.Success(200));

            var result = await _controller.Delete(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }
    }
}

