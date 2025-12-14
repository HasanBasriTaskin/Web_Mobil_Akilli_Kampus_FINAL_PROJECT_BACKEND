using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CampusContext _context;

        public IStudentDal Students { get; }
        public IFacultyDal Faculties { get; }
        public IDepartmentDal Departments { get; }

        public IRefreshTokenDal RefreshTokens { get; }
        public IPasswordResetTokenDal PasswordResetTokens { get; }
        public IEmailVerificationTokenDal EmailVerificationTokens { get; }
        
        // Academic Management
        public ICourseDal Courses { get; }
        public ICourseSectionDal CourseSections { get; }
        public IEnrollmentDal Enrollments { get; }
        public IAttendanceSessionDal AttendanceSessions { get; }
        public IAttendanceRecordDal AttendanceRecords { get; }
        public IExcuseRequestDal ExcuseRequests { get; }
        public IClassroomDal Classrooms { get; }
        public IAcademicCalendarDal AcademicCalendars { get; }
        public IAnnouncementDal Announcements { get; }

        public UnitOfWork(CampusContext context)
        {
            _context = context;
            
            // Initialize Repositories
            Students = new EfStudentDal(_context);
            Faculties = new EfFacultyDal(_context);
            Departments = new EfDepartmentDal(_context);
            
            RefreshTokens = new EfRefreshTokenDal(_context);
            PasswordResetTokens = new EfPasswordResetTokenDal(_context);
            EmailVerificationTokens = new EfEmailVerificationTokenDal(_context);
            
            // Academic Management Repositories
            Courses = new EfCourseDal(_context);
            CourseSections = new EfCourseSectionDal(_context);
            Enrollments = new EfEnrollmentDal(_context);
            AttendanceSessions = new EfAttendanceSessionDal(_context);
            AttendanceRecords = new EfAttendanceRecordDal(_context);
            ExcuseRequests = new EfExcuseRequestDal(_context);
            Classrooms = new EfClassroomDal(_context);
            AcademicCalendars = new EfAcademicCalendarDal(_context);
            Announcements = new EfAnnouncementDal(_context);
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
