namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class AttendanceSessionCreateDto
    {
        public int SectionId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? GeofenceRadius { get; set; } // Default: 15m if not provided
    }
}

