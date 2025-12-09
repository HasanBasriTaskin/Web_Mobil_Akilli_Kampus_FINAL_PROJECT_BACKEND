using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Repositories
{
    public class EfDepartmentDalTests
    {
        private readonly DbContextOptions<CampusContext> _dbContextOptions;

        public EfDepartmentDalTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private CampusContext CreateContext() => new CampusContext(_dbContextOptions);

        [Fact]
        public async Task AddAsync_ShouldAddDepartment()
        {
            using var context = CreateContext();
            var dal = new EfDepartmentDal(context);
            var dept = new Department { Name = "Computer Eng", Code = "CE" };

            await dal.AddAsync(dept);
            await context.SaveChangesAsync();

            var saved = await context.Departments.FirstOrDefaultAsync(d => d.Code == "CE");
            saved.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDepartment()
        {
            using var context = CreateContext();
            var dal = new EfDepartmentDal(context);
            var dept = new Department { Name = "Electrical Eng", Code = "EE" };
            await context.Departments.AddAsync(dept);
            await context.SaveChangesAsync();

            var result = await dal.GetByIdAsync(dept.Id);
            result.Should().NotBeNull();
            result.Code.Should().Be("EE");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllDepartments()
        {
            using var context = CreateContext();
            var dal = new EfDepartmentDal(context);
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            await context.Departments.AddRangeAsync(
                new Department { Name = "Civil Eng", Code = "CE" },
                new Department { Name = "Industrial Eng", Code = "IE" }
            );
            await context.SaveChangesAsync();

            var result = await dal.GetAllAsync();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Update_ShouldUpdateDepartment()
        {
            using var context = CreateContext();
            var dal = new EfDepartmentDal(context);
            var dept = new Department { Name = "Mechanical Eng", Code = "ME" };
            await context.Departments.AddAsync(dept);
            await context.SaveChangesAsync();

            dept.Name = "Mechanical Engineering";
            dal.Update(dept);
            await context.SaveChangesAsync();

            var updated = await context.Departments.FindAsync(dept.Id);
            updated!.Name.Should().Be("Mechanical Engineering");
        }

        [Fact]
        public async Task Remove_ShouldDeleteDepartment()
        {
            using var context = CreateContext();
            var dal = new EfDepartmentDal(context);
            var dept = new Department { Name = "Mining Eng", Code = "MINE" };
            await context.Departments.AddAsync(dept);
            await context.SaveChangesAsync();

            dal.Remove(dept);
            await context.SaveChangesAsync();

            var count = await context.Departments.CountAsync();
            count.Should().Be(0);
        }
    }
}
