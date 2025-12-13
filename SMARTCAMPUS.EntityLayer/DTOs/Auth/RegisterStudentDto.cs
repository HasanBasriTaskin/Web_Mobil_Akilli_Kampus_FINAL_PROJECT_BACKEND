namespace SMARTCAMPUS.EntityLayer.DTOs.Auth
{
    public class RegisterStudentDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
        
        // Student Specific
        public string StudentNumber { get; set; } = null!;
        public int DepartmentId { get; set; }
    }
}
