using Microsoft.AspNetCore.Identity;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Role : IdentityRole
    {
        public string? Description { get; set; }
    }
}
