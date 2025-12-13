namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int Credits { get; set; }
        public int ECTS { get; set; }
        public string? SyllabusUrl { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? DepartmentCode { get; set; }
        public List<string>? Prerequisites { get; set; } // Course codes
    }
}



