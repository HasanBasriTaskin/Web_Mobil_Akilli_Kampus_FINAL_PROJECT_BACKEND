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

        [Fact]
        public async Task CanInitializeAndUse()
        {
             using var context = new CampusContext(_dbContextOptions);
             var dal = new EfFacultyDal(context);

             await dal.AddAsync(new Faculty { UserId = "u1", DepartmentId = 1, EmployeeNumber = "123", Title = "Prof" });
             await context.SaveChangesAsync();

             var all = await dal.GetAllAsync();
             all.Should().HaveCount(1);
        }
    }
}
