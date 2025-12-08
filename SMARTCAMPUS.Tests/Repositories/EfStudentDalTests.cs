using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Repositories
{
    public class EfStudentDalTests
    {
        private readonly DbContextOptions<CampusContext> _dbContextOptions;

        public EfStudentDalTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task CanInitializeAndUse()
        {
             using var context = new CampusContext(_dbContextOptions);
             var dal = new EfStudentDal(context);

             await dal.AddAsync(new Student { StudentNumber = "123", UserId = "u1", DepartmentId = 1 });
             await context.SaveChangesAsync();

             var all = await dal.GetAllAsync();
             all.Should().HaveCount(1);
        }
    }
}
