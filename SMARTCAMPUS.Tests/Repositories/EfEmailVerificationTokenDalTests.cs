using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Repositories
{
    public class EfEmailVerificationTokenDalTests
    {
        private readonly DbContextOptions<CampusContext> _dbContextOptions;

        public EfEmailVerificationTokenDalTests()
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
            var dal = new EfEmailVerificationTokenDal(context);
            var token = new EmailVerificationToken { Token = "email-token-1", UserId = "u1", ExpiresAt = DateTime.Now.AddDays(1) };

            await dal.AddAsync(token);
            await context.SaveChangesAsync();

            var saved = await context.EmailVerificationTokens.FirstOrDefaultAsync(t => t.Token == "email-token-1");
            saved.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnToken()
        {
            using var context = CreateContext();
            var dal = new EfEmailVerificationTokenDal(context);
            var token = new EmailVerificationToken { Token = "email-token-2", UserId = "u2", ExpiresAt = DateTime.Now.AddDays(1) };
            await context.EmailVerificationTokens.AddAsync(token);
            await context.SaveChangesAsync();

            var result = await dal.GetByIdAsync(token.Id);
            result.Should().NotBeNull();
            result.Token.Should().Be("email-token-2");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTokens()
        {
            using var context = CreateContext();
            var dal = new EfEmailVerificationTokenDal(context);
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            await context.EmailVerificationTokens.AddRangeAsync(
                new EmailVerificationToken { Token = "t1", UserId = "u1", ExpiresAt = DateTime.Now.AddDays(1) },
                new EmailVerificationToken { Token = "t2", UserId = "u2", ExpiresAt = DateTime.Now.AddDays(1) }
            );
            await context.SaveChangesAsync();

            var result = await dal.GetAllAsync();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Update_ShouldUpdateToken()
        {
            using var context = CreateContext();
            var dal = new EfEmailVerificationTokenDal(context);
            var token = new EmailVerificationToken { Token = "email-token-3", UserId = "u3", ExpiresAt = DateTime.Now.AddDays(1) };
            await context.EmailVerificationTokens.AddAsync(token);
            await context.SaveChangesAsync();

            token.ExpiresAt = DateTime.Now.AddDays(2);
            dal.Update(token);
            await context.SaveChangesAsync();

            var updated = await context.EmailVerificationTokens.FindAsync(token.Id);
            updated!.ExpiresAt.Date.Should().Be(token.ExpiresAt.Date);
        }

        [Fact]
        public async Task Remove_ShouldDeleteToken()
        {
            using var context = CreateContext();
            var dal = new EfEmailVerificationTokenDal(context);
            var token = new EmailVerificationToken { Token = "email-token-4", UserId = "u4", ExpiresAt = DateTime.Now.AddDays(1) };
            await context.EmailVerificationTokens.AddAsync(token);
            await context.SaveChangesAsync();

            dal.Remove(token);
            await context.SaveChangesAsync();

            var count = await context.EmailVerificationTokens.CountAsync();
            count.Should().Be(0);
        }
    }
}
