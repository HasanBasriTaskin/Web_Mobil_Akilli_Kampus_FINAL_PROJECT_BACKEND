using SMARTCAMPUS.EntityLayer.DTOs.User;

namespace SMARTCAMPUS.EntityLayer.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime AccessTokenExpiration { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
        public UserDto User { get; set; } = null!;
    }
}
