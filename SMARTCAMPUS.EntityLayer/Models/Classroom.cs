using System.ComponentModel.DataAnnotations;

namespace SMARTCAMPUS.EntityLayer.Models
{
    public class Classroom : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Building { get; set; } = null!;
        
        [Required]
        [MaxLength(20)]
        public string RoomNumber { get; set; } = null!;
        
        [Range(1, 1000)]
        public int Capacity { get; set; }
        
        // Features stored as JSON: ["projector", "smartboard", "computer_lab", "air_conditioning"]
        public string? FeaturesJson { get; set; }
        
        // GPS Coordinates for attendance geofencing
        [Range(-90, 90)]
        public double Latitude { get; set; }
        
        [Range(-180, 180)]
        public double Longitude { get; set; }
    }
}
