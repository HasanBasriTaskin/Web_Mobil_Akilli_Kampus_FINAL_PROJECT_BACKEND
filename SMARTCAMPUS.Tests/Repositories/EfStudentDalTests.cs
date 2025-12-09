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

        private CampusContext CreateContext() => new CampusContext(_dbContextOptions);

        [Fact]
        public async Task AddAsync_ShouldAddStudent()
        {
            using var context = CreateContext();
            var dal = new EfStudentDal(context);
            var student = new Student { StudentNumber = "S100", UserId = "u1", DepartmentId = 1 };

            await dal.AddAsync(student);
            await context.SaveChangesAsync();

            var saved = await context.Students.FirstOrDefaultAsync(s => s.StudentNumber == "S100");
            saved.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnStudent()
        {
            using var context = CreateContext();
            var dal = new EfStudentDal(context);
            var student = new Student { StudentNumber = "S101", UserId = "u2", DepartmentId = 1 };
            await context.Students.AddAsync(student);
            await context.SaveChangesAsync();

            var result = await dal.GetByIdAsync(student.Id);
            result.Should().NotBeNull();
            result.StudentNumber.Should().Be("S101");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllStudents()
        {
            using var context = CreateContext();
            var dal = new EfStudentDal(context);
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            await context.Students.AddRangeAsync(
                new Student { StudentNumber = "S201", UserId = "u3", DepartmentId = 1 },
                new Student { StudentNumber = "S202", UserId = "u4", DepartmentId = 1 }
            );
            await context.SaveChangesAsync();

            var result = await dal.GetAllAsync();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Update_ShouldUpdateStudent()
        {
            using var context = CreateContext();
            var dal = new EfStudentDal(context);
            var student = new Student { StudentNumber = "S300", UserId = "u5", DepartmentId = 1 };
            await context.Students.AddAsync(student);
            await context.SaveChangesAsync();

            student.StudentNumber = "S300-UPDATED";
            dal.Update(student);
            await context.SaveChangesAsync();

            var updated = await context.Students.FindAsync(student.Id);
            updated!.StudentNumber.Should().Be("S300-UPDATED");
        }

        [Fact]
        public async Task Remove_ShouldDeleteStudent()
        {
            using var context = CreateContext();
            var dal = new EfStudentDal(context);
            var student = new Student { StudentNumber = "S400", UserId = "u6", DepartmentId = 1 };
            await context.Students.AddAsync(student);
            await context.SaveChangesAsync();

            dal.Remove(student);
            await context.SaveChangesAsync();

            var count = await context.Students.CountAsync();
            count.Should().Be(0);
        }
    }
}
