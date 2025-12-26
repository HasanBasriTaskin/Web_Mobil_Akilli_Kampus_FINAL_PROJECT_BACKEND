using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Repositories
{
    public class GenericRepositoryTests
    {
        private readonly DbContextOptions<CampusContext> _dbContextOptions;

        public GenericRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        // Create a concrete implementation or use a simple entity for testing
        // Since GenericRepository is generic, we can test it with one of the existing entities, e.g., Department.

        [Fact]
        public async Task AddAsync_ShouldAddEntity()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var repository = new GenericRepository<Department>(context);
            var department = new Department { Name = "Computer Science", Description = "CS Dept", Code = "CS" };

            // Act
            await repository.AddAsync(department);
            await context.SaveChangesAsync();

            // Assert
            var savedDepartment = await context.Departments.FirstOrDefaultAsync();
            savedDepartment.Should().NotBeNull();
            savedDepartment!.Name.Should().Be("Computer Science");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var repository = new GenericRepository<Department>(context);
            var department = new Department { Name = "Math", Description = "Math Dept", Code = "MATH" };
            await context.Departments.AddAsync(department);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(department.Id);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Math");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEntities()
        {
             // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var repository = new GenericRepository<Department>(context);
            await context.Departments.AddRangeAsync(
                new Department { Name = "D1", Code = "D1" },
                new Department { Name = "D2", Code = "D2" }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Update_ShouldUpdateEntity()
        {
             // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var repository = new GenericRepository<Department>(context);
            var department = new Department { Name = "Old Name", Code = "OLD" };
            await context.Departments.AddAsync(department);
            await context.SaveChangesAsync();

            // Act
            department.Name = "New Name";
            repository.Update(department);
            await context.SaveChangesAsync();

            // Assert
            var updated = await context.Departments.FindAsync(department.Id);
            updated!.Name.Should().Be("New Name");
        }

        [Fact]
        public async Task Remove_ShouldRemoveEntity()
        {
             // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var repository = new GenericRepository<Department>(context);
            var department = new Department { Name = "To Remove", Code = "REM" };
            await context.Departments.AddAsync(department);
            await context.SaveChangesAsync();

            // Act
            repository.Remove(department);
            await context.SaveChangesAsync();

            // Assert
            var count = await context.Departments.CountAsync();
            count.Should().Be(0);
        }

        [Fact]
        public async Task GetPagedAsync_ShouldReturnPagedResults()
        {
             // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var repository = new GenericRepository<Department>(context);
            var departments = new List<Department>();
            for(int i=1; i<=10; i++)
            {
                departments.Add(new Department { Name = $"D{i}", Code = $"C{i}" });
            }
            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();

            // Act
            var page1 = await repository.GetPagedAsync(1, 3);
            var page2 = await repository.GetPagedAsync(2, 3);

            // Assert
            page1.Should().HaveCount(3);
            page1.First().Name.Should().Be("D1");

            page2.Should().HaveCount(3);
            page2.First().Name.Should().Be("D4");
        }

        [Fact]
        public async Task AddRangeAsync_ShouldAddMultipleEntities()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var repository = new GenericRepository<Department>(context);
            var departments = new List<Department>
            {
                new Department { Name = "D1", Code = "C1" },
                new Department { Name = "D2", Code = "C2" },
                new Department { Name = "D3", Code = "C3" }
            };

            // Act
            await repository.AddRangeAsync(departments);
            await context.SaveChangesAsync();

            // Assert
            var count = await context.Departments.CountAsync();
            count.Should().Be(3);
        }

        [Fact]
        public void Where_ShouldReturnFilteredQueryable()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var repository = new GenericRepository<Department>(context);
            context.Departments.AddRange(
                new Department { Name = "Computer Science", Code = "CS" },
                new Department { Name = "Computer Engineering", Code = "CE" },
                new Department { Name = "Mathematics", Code = "MTH" }
            );
            context.SaveChanges();

            // Act
            var result = repository.Where(d => d.Name.Contains("Computer")).ToList();

            // Assert
            result.Should().HaveCount(2);
            result.All(d => d.Name.Contains("Computer")).Should().BeTrue();
        }

        [Fact]
        public async Task RemoveRange_ShouldRemoveMultipleEntities()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var repository = new GenericRepository<Department>(context);
            var departments = new List<Department>
            {
                new Department { Name = "D1", Code = "C1" },
                new Department { Name = "D2", Code = "C2" },
                new Department { Name = "D3", Code = "C3" }
            };
            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();

            // Act
            repository.RemoveRange(departments);
            await context.SaveChangesAsync();

            // Assert
            var count = await context.Departments.CountAsync();
            count.Should().Be(0);
        }

        [Fact]
        public async Task AnyAsync_ShouldReturnTrue_WhenEntityExists()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var repository = new GenericRepository<Department>(context);
            await context.Departments.AddAsync(new Department { Name = "Computer Science", Code = "CS" });
            await context.SaveChangesAsync();

            // Act
            var result = await repository.AnyAsync(d => d.Code == "CS");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AnyAsync_ShouldReturnFalse_WhenNoEntityMatches()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var repository = new GenericRepository<Department>(context);
            await context.Departments.AddAsync(new Department { Name = "Computer Science", Code = "CS" });
            await context.SaveChangesAsync();

            // Act
            var result = await repository.AnyAsync(d => d.Code == "NONEXISTENT");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetPagedAsync_WithFilter_ShouldReturnFilteredPagedResults()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var repository = new GenericRepository<Department>(context);
            var departments = new List<Department>();
            for (int i = 1; i <= 10; i++)
            {
                departments.Add(new Department { Name = $"Department {i}", Code = $"D{i}" });
            }
            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetPagedAsync(d => d.Id > 3, 1, 3);

            // Assert
            result.Should().HaveCount(3);
            result.All(d => d.Id > 3).Should().BeTrue();
        }
    }
}
