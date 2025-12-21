namespace SMARTCAMPUS.EntityLayer.DTOs.Scheduling
{
    public class ScheduleConflictDto
    {
        public bool HasConflict { get; set; }
        public string? ConflictMessage { get; set; }
        public List<ScheduleDto> ConflictingSchedules { get; set; } = new();
    }
}
