namespace SMARTCAMPUS.EntityLayer.DTOs.Scheduling
{
    public class ClassroomReservationCreateDto
    {
        public int ClassroomId { get; set; }
        public string? StudentLeaderName { get; set; }
        public string Purpose { get; set; } = null!;
        public DateTime ReservationDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
