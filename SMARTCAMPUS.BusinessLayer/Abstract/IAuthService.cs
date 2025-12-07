using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IAuthService
    {
        Task<Response<TokenDto>> LoginAsync(LoginDto loginDto);
        Task<Response<TokenDto>> RegisterStudentAsync(RegisterStudentDto registerDto);
        Task<Response<TokenDto>> CreateTokenByRefreshTokenAsync(string refreshToken); 
        Task<Response<NoDataDto>> RevokeRefreshTokenAsync(string refreshToken);
        
        // We can add those later as per plan "Authentication Module" tasks.
        // For now starting with Login - Register.
    }
}
