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

        Task CommitAsync();
        void Commit();
        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();
    }
}
