namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class CourseCreateDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int Credits { get; set; }
        public int ECTS { get; set; }
        public string? SyllabusUrl { get; set; }
        public int DepartmentId { get; set; }
        public List<string>? PrerequisiteCodes { get; set; } // Course codes that are prerequisites
    }
}

