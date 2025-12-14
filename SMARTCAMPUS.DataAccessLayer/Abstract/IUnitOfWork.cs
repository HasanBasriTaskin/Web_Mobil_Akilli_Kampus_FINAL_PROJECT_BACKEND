namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IStudentDal Students { get; }
        IFacultyDal Faculties { get; }
        IDepartmentDal Departments { get; }
        
        IRefreshTokenDal RefreshTokens { get; }
        IPasswordResetTokenDal PasswordResetTokens { get; }
        IEmailVerificationTokenDal EmailVerificationTokens { get; }
        
        // Academic Management
        ICourseDal Courses { get; }
        ICourseSectionDal CourseSections { get; }
        IEnrollmentDal Enrollments { get; }
        IAttendanceSessionDal AttendanceSessions { get; }
        IAttendanceRecordDal AttendanceRecords { get; }
        IExcuseRequestDal ExcuseRequests { get; }
        IClassroomDal Classrooms { get; }
        IAcademicCalendarDal AcademicCalendars { get; }
        IAnnouncementDal Announcements { get; }

        Task CommitAsync();
        void Commit();
        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();
    }
}
