namespace SMARTCAMPUS.EntityLayer.DTOs.Auth
{
    public class LoginUserDto
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string UserType { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsEmailVerified { get; set; }
        public bool IsActive { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Student veya Faculty bilgileri (rol bazlı)
        public StudentInfoDto? Student { get; set; }
        public FacultyInfoDto? Faculty { get; set; }
    }
}
