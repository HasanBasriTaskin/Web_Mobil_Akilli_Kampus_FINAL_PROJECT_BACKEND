namespace SMARTCAMPUS.EntityLayer.DTOs.Scheduling
{
    public class ScheduleConflictDto
    {
        public bool HasConflict { get; set; }
        public string? ConflictType { get; set; }
        public int? ConflictingScheduleId { get; set; }
        public string? ConflictingCourse { get; set; }
        public string? Message { get; set; }
        public List<ScheduleDto> ConflictingSchedules { get; set; } = new();
    }
}
