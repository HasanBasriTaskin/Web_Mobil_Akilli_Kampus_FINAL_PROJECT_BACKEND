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

        private CampusContext CreateContext() => new CampusContext(_dbContextOptions);

        [Fact]
        public async Task AddAsync_ShouldAddToken()
        {
            using var context = CreateContext();
            var dal = new EfRefreshTokenDal(context);
            var token = new RefreshToken { Token = "abc", UserId = "u1", Expires = DateTime.Now.AddDays(1) };

            await dal.AddAsync(token);
            await context.SaveChangesAsync();

            var saved = await context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == "abc");
            saved.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnToken()
        {
            using var context = CreateContext();
            var dal = new EfRefreshTokenDal(context);
            var token = new RefreshToken { Token = "def", UserId = "u2", Expires = DateTime.Now.AddDays(1) };
            await context.RefreshTokens.AddAsync(token);
            await context.SaveChangesAsync();

            var result = await dal.GetByIdAsync(token.Id);
            result.Should().NotBeNull();
            result.Token.Should().Be("def");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTokens()
        {
            using var context = CreateContext();
            var dal = new EfRefreshTokenDal(context);
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            await context.RefreshTokens.AddRangeAsync(
                new RefreshToken { Token = "t1", UserId = "u1", Expires = DateTime.Now.AddDays(1) },
                new RefreshToken { Token = "t2", UserId = "u2", Expires = DateTime.Now.AddDays(1) }
            );
            await context.SaveChangesAsync();

            var result = await dal.GetAllAsync();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Update_ShouldUpdateToken()
        {
            using var context = CreateContext();
            var dal = new EfRefreshTokenDal(context);
            var token = new RefreshToken { Token = "ghi", UserId = "u3", Expires = DateTime.Now.AddDays(1) };
            await context.RefreshTokens.AddAsync(token);
            await context.SaveChangesAsync();

            token.Revoked = DateTime.Now;
            dal.Update(token);
            await context.SaveChangesAsync();

            var updated = await context.RefreshTokens.FindAsync(token.Id);
            updated!.Revoked.Should().NotBeNull();
        }

        [Fact]
        public async Task Remove_ShouldDeleteToken()
        {
            using var context = CreateContext();
            var dal = new EfRefreshTokenDal(context);
            var token = new RefreshToken { Token = "jkl", UserId = "u4", Expires = DateTime.Now.AddDays(1) };
            await context.RefreshTokens.AddAsync(token);
            await context.SaveChangesAsync();

            dal.Remove(token);
            await context.SaveChangesAsync();

            var count = await context.RefreshTokens.CountAsync();
            count.Should().Be(0);
        }
    }
}
