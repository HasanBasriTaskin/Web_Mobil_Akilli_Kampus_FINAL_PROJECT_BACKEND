namespace SMARTCAMPUS.EntityLayer.DTOs.User
{
    public class UserProfileDto
    {
        public int Id { get; set; } // IdentityUser<int> kullanmasak da int Id ile dönebiliriz veya string Id. 
                                    // User entity'si string IdentityUser yaptık.
        public string IdString { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ProfilePictureUrl { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
