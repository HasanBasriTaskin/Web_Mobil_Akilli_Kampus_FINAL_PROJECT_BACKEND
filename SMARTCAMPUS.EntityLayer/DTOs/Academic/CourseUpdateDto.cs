namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class CourseUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Credits { get; set; }
        public int? ECTS { get; set; }
        public string? SyllabusUrl { get; set; }
        public int? DepartmentId { get; set; }
        public List<string>? PrerequisiteCodes { get; set; } // Course codes that are prerequisites
    }
}

