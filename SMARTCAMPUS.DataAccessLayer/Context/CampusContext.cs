using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.EntityLayer.Models;
using System.Reflection;

namespace SMARTCAMPUS.DataAccessLayer.Context
{
    public class CampusContext : IdentityDbContext<User, Role, string>
    {
        public CampusContext(DbContextOptions<CampusContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Department> Departments { get; set; }
        
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
        
        // Academic Management
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseSection> CourseSections { get; set; }
        public DbSet<CoursePrerequisite> CoursePrerequisites { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<AttendanceSession> AttendanceSessions { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<ExcuseRequest> ExcuseRequests { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Apply all configurations from the current assembly (Configurations folder)
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Additional custom Identity adjustments if needed
            builder.Entity<User>().ToTable("Users");
            builder.Entity<Role>().ToTable("Roles");
        }
    }
}
