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
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}
