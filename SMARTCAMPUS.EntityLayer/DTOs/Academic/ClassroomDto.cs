namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class ClassroomDto
    {
        public int Id { get; set; }
        public string Building { get; set; } = null!;
        public string RoomNumber { get; set; } = null!;
        public int Capacity { get; set; }
        public string? FeaturesJson { get; set; }
        public string? FullName => $"{Building}-{RoomNumber}";
    }
}



