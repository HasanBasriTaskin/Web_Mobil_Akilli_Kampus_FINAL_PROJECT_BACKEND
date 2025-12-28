using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        // Part 1 - Core
        IStudentDal Students { get; }
        IFacultyDal Faculties { get; }
        IDepartmentDal Departments { get; }
        
        IRefreshTokenDal RefreshTokens { get; }
        IPasswordResetTokenDal PasswordResetTokens { get; }
        IEmailVerificationTokenDal EmailVerificationTokens { get; }
        IUserDal Users { get; }
        
        // Part 2 - Academic Management
        ICourseDal Courses { get; }
        ICourseSectionDal CourseSections { get; }
        ICoursePrerequisiteDal CoursePrerequisites { get; }
        IEnrollmentDal Enrollments { get; }
        IAttendanceSessionDal AttendanceSessions { get; }
        IAttendanceRecordDal AttendanceRecords { get; }
        IExcuseRequestDal ExcuseRequests { get; }
        IClassroomDal Classrooms { get; }

        // Part 3 - Meal Management
        ICafeteriaDal Cafeterias { get; }
        IFoodItemDal FoodItems { get; }
        IMealMenuDal MealMenus { get; }
        IMealMenuItemDal MealMenuItems { get; }
        IMealNutritionDal MealNutritions { get; }
        IMealReservationDal MealReservations { get; }

        // Part 3 - Wallet Management
        IWalletDal Wallets { get; }
        IWalletTransactionDal WalletTransactions { get; }

        // Part 3 - Event Management
        IEventCategoryDal EventCategories { get; }
        IEventDal Events { get; }
        IEventRegistrationDal EventRegistrations { get; }
        IEventWaitlistDal EventWaitlists { get; }

        // Part 3 - Scheduling
        IScheduleDal Schedules { get; }
        IClassroomReservationDal ClassroomReservations { get; }

        // Faculty Course Assignment
        IFacultyRequestDal FacultyRequests { get; }

        Task CommitAsync();
        void Commit();
        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();
    }
}
