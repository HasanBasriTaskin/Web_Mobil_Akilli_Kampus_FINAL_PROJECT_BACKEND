namespace SMARTCAMPUS.EntityLayer.DTOs.Course
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
        public string DepartmentName { get; set; } = null!;
        public List<CoursePrerequisiteDto> Prerequisites { get; set; } = new();
    }

    public class CoursePrerequisiteDto
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
    }

    public class CreateCourseDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int Credits { get; set; }
        public int ECTS { get; set; }
        public string? SyllabusUrl { get; set; }
        public int DepartmentId { get; set; }
        public List<int>? PrerequisiteIds { get; set; }
    }

    public class UpdateCourseDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Credits { get; set; }
        public int? ECTS { get; set; }
        public string? SyllabusUrl { get; set; }
        public List<int>? PrerequisiteIds { get; set; }
    }

    public class CourseListDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Credits { get; set; }
        public int ECTS { get; set; }
        public string DepartmentName { get; set; } = null!;
    }
}
