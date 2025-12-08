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

        [Fact]
        public async Task CanInitializeAndUse()
        {
             using var context = new CampusContext(_dbContextOptions);
             var dal = new EfDepartmentDal(context);

             await dal.AddAsync(new Department { Name = "Test", Code = "TEST" });
             await context.SaveChangesAsync();

             var count = await dal.GetAllAsync();
             count.Should().HaveCount(1);
        }
    }
}
