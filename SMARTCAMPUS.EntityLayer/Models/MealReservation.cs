using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class MealReservation : BaseEntity
    {
        [Required]
        public string UserId { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        
        public int MenuId { get; set; }
        
        [ForeignKey("MenuId")]
        public MealMenu Menu { get; set; } = null!;
        
        public int CafeteriaId { get; set; }
        
        [ForeignKey("CafeteriaId")]
        public Cafeteria Cafeteria { get; set; } = null!;
        
        public MealType MealType { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string QRCode { get; set; } = null!; // GUID string
        
        public MealReservationStatus Status { get; set; } = MealReservationStatus.Reserved;
        
        public DateTime? UsedAt { get; set; }
    }
}
