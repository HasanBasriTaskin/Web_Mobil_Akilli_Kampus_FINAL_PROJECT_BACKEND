using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Scheduling
{
    public class ClassroomReservationDto
    {
        public int Id { get; set; }
        public int ClassroomId { get; set; }
        public string ClassroomInfo { get; set; } = null!;
        public string RequestedByUserId { get; set; } = null!;
        public string RequestedByName { get; set; } = null!;
        public string? StudentLeaderName { get; set; }
        public string Purpose { get; set; } = null!;
        public DateTime ReservationDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
        public ReservationStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreatedAt { get; set; }
    }
}
