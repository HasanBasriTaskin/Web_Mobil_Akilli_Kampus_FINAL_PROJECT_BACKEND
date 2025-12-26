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
        
        // Part 2 - Academic Management
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseSection> CourseSections { get; set; }
        public DbSet<CoursePrerequisite> CoursePrerequisites { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        
        // Part 2 - Attendance System
        public DbSet<AttendanceSession> AttendanceSessions { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<ExcuseRequest> ExcuseRequests { get; set; }
        
        // Part 3 - Meal & Cafeteria
        public DbSet<Cafeteria> Cafeterias { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<MealMenu> MealMenus { get; set; }
        public DbSet<MealMenuItem> MealMenuItems { get; set; }
        public DbSet<MealNutrition> MealNutritions { get; set; }
        public DbSet<MealReservation> MealReservations { get; set; }
        
        // Part 3 - Wallet
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        
        // Part 3 - Events
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }
        public DbSet<EventWaitlist> EventWaitlists { get; set; }
        
        // Part 3 - Scheduling
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ClassroomReservation> ClassroomReservations { get; set; }

        // Part 4 - Notifications
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationPreference> NotificationPreferences { get; set; }

        // Part 4 - IoT Sensors
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<SensorReading> SensorReadings { get; set; }

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
