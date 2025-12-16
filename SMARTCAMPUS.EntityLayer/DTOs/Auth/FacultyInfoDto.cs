namespace SMARTCAMPUS.EntityLayer.DTOs.Auth
{
    public class FacultyInfoDto
    {
        public string EmployeeNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int DepartmentId { get; set; }
        public string? OfficeLocation { get; set; }
    }
}
