using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CampusContext _context;

        // Part 1 - Core
        public IStudentDal Students { get; }
        public IFacultyDal Faculties { get; }
        public IDepartmentDal Departments { get; }

        public IRefreshTokenDal RefreshTokens { get; }
        public IPasswordResetTokenDal PasswordResetTokens { get; }
        public IEmailVerificationTokenDal EmailVerificationTokens { get; }
        public IUserDal Users { get; }
        
        // Part 2 - Academic Management
        public ICourseDal Courses { get; }
        public ICourseSectionDal CourseSections { get; }
        public ICoursePrerequisiteDal CoursePrerequisites { get; }
        public IEnrollmentDal Enrollments { get; }
        public IAttendanceSessionDal AttendanceSessions { get; }
        public IAttendanceRecordDal AttendanceRecords { get; }
        public IExcuseRequestDal ExcuseRequests { get; }
        public IClassroomDal Classrooms { get; }

        // Part 3 - Meal Management
        public ICafeteriaDal Cafeterias { get; }
        public IFoodItemDal FoodItems { get; }
        public IMealMenuDal MealMenus { get; }
        public IMealMenuItemDal MealMenuItems { get; }
        public IMealNutritionDal MealNutritions { get; }
        public IMealReservationDal MealReservations { get; }

        // Part 3 - Wallet Management
        public IWalletDal Wallets { get; }
        public IWalletTransactionDal WalletTransactions { get; }

        // Part 3 - Event Management
        public IEventCategoryDal EventCategories { get; }
        public IEventDal Events { get; }
        public IEventRegistrationDal EventRegistrations { get; }
        public IEventWaitlistDal EventWaitlists { get; }

        // Part 3 - Scheduling
        public IScheduleDal Schedules { get; }
        public IClassroomReservationDal ClassroomReservations { get; }

        public UnitOfWork(CampusContext context)
        {
            _context = context;
            
            // Part 1 - Core Repositories
            Students = new EfStudentDal(_context);
            Faculties = new EfFacultyDal(_context);
            Departments = new EfDepartmentDal(_context);
            
            RefreshTokens = new EfRefreshTokenDal(_context);
            PasswordResetTokens = new EfPasswordResetTokenDal(_context);
            EmailVerificationTokens = new EfEmailVerificationTokenDal(_context);
            Users = new EfUserDal(_context);
            
            // Part 2 - Academic Management Repositories
            Courses = new EfCourseDal(_context);
            CourseSections = new EfCourseSectionDal(_context);
            CoursePrerequisites = new EfCoursePrerequisiteDal(_context);
            Enrollments = new EfEnrollmentDal(_context);
            AttendanceSessions = new EfAttendanceSessionDal(_context);
            AttendanceRecords = new EfAttendanceRecordDal(_context);
            ExcuseRequests = new EfExcuseRequestDal(_context);
            Classrooms = new EfClassroomDal(_context);

            // Part 3 - Meal Management Repositories
            Cafeterias = new EfCafeteriaDal(_context);
            FoodItems = new EfFoodItemDal(_context);
            MealMenus = new EfMealMenuDal(_context);
            MealMenuItems = new EfMealMenuItemDal(_context);
            MealNutritions = new EfMealNutritionDal(_context);
            MealReservations = new EfMealReservationDal(_context);

            // Part 3 - Wallet Management Repositories
            Wallets = new EfWalletDal(_context);
            WalletTransactions = new EfWalletTransactionDal(_context);

            // Part 3 - Event Management Repositories
            EventCategories = new EfEventCategoryDal(_context);
            Events = new EfEventDal(_context);
            EventRegistrations = new EfEventRegistrationDal(_context);
            EventWaitlists = new EfEventWaitlistDal(_context);

            // Part 3 - Scheduling Repositories
            Schedules = new EfScheduleDal(_context);
            ClassroomReservations = new EfClassroomReservationDal(_context);
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}
