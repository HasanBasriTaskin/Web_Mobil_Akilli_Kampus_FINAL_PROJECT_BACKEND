using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Repositories
{
    public class EfFacultyDalTests
    {
        private readonly DbContextOptions<CampusContext> _dbContextOptions;

        public EfFacultyDalTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private CampusContext CreateContext() => new CampusContext(_dbContextOptions);

        [Fact]
        public async Task AddAsync_ShouldAddFaculty()
        {
            using var context = CreateContext();
            var dal = new EfFacultyDal(context);
            var faculty = new Faculty { EmployeeNumber = "F100", UserId = "u1", DepartmentId = 1, Title = "Dr" };

            await dal.AddAsync(faculty);
            await context.SaveChangesAsync();

            var saved = await context.Faculties.FirstOrDefaultAsync(f => f.EmployeeNumber == "F100");
            saved.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnFaculty()
        {
            using var context = CreateContext();
            var dal = new EfFacultyDal(context);
            var faculty = new Faculty { EmployeeNumber = "F101", UserId = "u2", DepartmentId = 1, Title = "Prof" };
            await context.Faculties.AddAsync(faculty);
            await context.SaveChangesAsync();

            var result = await dal.GetByIdAsync(faculty.Id);
            result.Should().NotBeNull();
            result.EmployeeNumber.Should().Be("F101");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllFaculties()
        {
            using var context = CreateContext();
            var dal = new EfFacultyDal(context);
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            await context.Faculties.AddRangeAsync(
                new Faculty { EmployeeNumber = "F201", UserId = "u3", DepartmentId = 1, Title = "Dr" },
                new Faculty { EmployeeNumber = "F202", UserId = "u4", DepartmentId = 1, Title = "Dr" }
            );
            await context.SaveChangesAsync();

            var result = await dal.GetAllAsync();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Update_ShouldUpdateFaculty()
        {
            using var context = CreateContext();
            var dal = new EfFacultyDal(context);
            var faculty = new Faculty { EmployeeNumber = "F300", UserId = "u5", DepartmentId = 1, Title = "Dr" };
            await context.Faculties.AddAsync(faculty);
            await context.SaveChangesAsync();

            faculty.Title = "Assoc Prof";
            dal.Update(faculty);
            await context.SaveChangesAsync();

            var updated = await context.Faculties.FindAsync(faculty.Id);
            updated!.Title.Should().Be("Assoc Prof");
        }

        [Fact]
        public async Task Remove_ShouldDeleteFaculty()
        {
            using var context = CreateContext();
            var dal = new EfFacultyDal(context);
            var faculty = new Faculty { EmployeeNumber = "F400", UserId = "u6", DepartmentId = 1, Title = "Dr" };
            await context.Faculties.AddAsync(faculty);
            await context.SaveChangesAsync();

            dal.Remove(faculty);
            await context.SaveChangesAsync();

            var count = await context.Faculties.CountAsync();
            count.Should().Be(0);
        }
    }
}
