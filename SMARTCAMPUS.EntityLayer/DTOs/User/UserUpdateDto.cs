namespace SMARTCAMPUS.EntityLayer.DTOs.User
{
    public class UserUpdateDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        
        // Admin might update roles here or separate endpoint? 
        // Usually separate endpoint is safer/cleaner for Roles.
    }
}
