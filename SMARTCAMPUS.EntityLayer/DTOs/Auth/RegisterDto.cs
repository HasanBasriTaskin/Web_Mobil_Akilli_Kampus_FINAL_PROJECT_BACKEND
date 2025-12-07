namespace SMARTCAMPUS.EntityLayer.DTOs.Auth
{
    public class RegisterDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
        
        // Optional: Role selection usually handled by logic, but explicitly requesting role might be needed for simple setups
        // For now, we assume default is Student or explicitly assigned by other means, 
        // but adding Role property for flexibility if Admin creates users.
        // public string? Role { get; set; } 
    }
}
