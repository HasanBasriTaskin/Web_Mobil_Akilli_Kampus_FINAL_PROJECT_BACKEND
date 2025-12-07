namespace SMARTCAMPUS.EntityLayer.DTOs.Auth
{
    public class ChangePasswordDto
    {
        public string UserId { get; set; } = null!; // Or get from claims
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
