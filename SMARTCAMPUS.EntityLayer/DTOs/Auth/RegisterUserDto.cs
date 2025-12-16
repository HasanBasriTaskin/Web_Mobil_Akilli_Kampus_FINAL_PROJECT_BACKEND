using System.ComponentModel.DataAnnotations;

namespace SMARTCAMPUS.EntityLayer.DTOs.Auth
{
    public class RegisterUserDto
    {
        [Required]
        public string UserType { get; set; } = null!; // "Student" or "Faculty"

        [Required]
        public string FullName { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = null!;
        
        [Required]
        public int DepartmentId { get; set; }

        // Student Specific
        public string? StudentNumber { get; set; }

        // Faculty Specific
        public string? EmployeeNumber { get; set; }
        public string? Title { get; set; }
        public string? OfficeLocation { get; set; }
    }
}
