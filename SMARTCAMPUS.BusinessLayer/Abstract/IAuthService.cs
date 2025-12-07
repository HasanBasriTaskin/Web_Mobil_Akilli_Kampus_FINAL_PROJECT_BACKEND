using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IAuthService
    {
        Task<Response<TokenDto>> LoginAsync(LoginDto loginDto);
        Task<Response<TokenDto>> RegisterAsync(RegisterStudentDto registerDto);
        Task<Response<TokenDto>> CreateTokenByRefreshTokenAsync(string refreshToken); 
        Task<Response<NoDataDto>> RevokeRefreshTokenAsync(string refreshToken);
        Task<Response<NoDataDto>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<Response<NoDataDto>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<Response<NoDataDto>> VerifyEmailAsync(string userId, string token);
        Task<Response<NoDataDto>> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
        Task<Response<NoDataDto>> LogoutAsync(string refreshToken);
        
        // We can add those later as per plan "Authentication Module" tasks.
        // For now starting with Login - Register.
    }
}
