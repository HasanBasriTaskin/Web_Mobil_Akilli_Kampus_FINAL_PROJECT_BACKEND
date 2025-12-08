using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Repositories
{
    public class EfRefreshTokenDalTests
    {
        private readonly DbContextOptions<CampusContext> _dbContextOptions;

        public EfRefreshTokenDalTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task CanInitializeAndUse()
        {
             using var context = new CampusContext(_dbContextOptions);
             var dal = new EfRefreshTokenDal(context);

             await dal.AddAsync(new RefreshToken { Token = "token", UserId = "u1" });
             await context.SaveChangesAsync();

             var all = await dal.GetAllAsync();
             all.Should().HaveCount(1);
        }
    }
}
