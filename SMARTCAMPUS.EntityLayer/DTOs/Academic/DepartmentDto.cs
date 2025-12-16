namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? FacultyName { get; set; }
        public string? Description { get; set; }
    }
}
