namespace SMARTCAMPUS.EntityLayer.DTOs.Auth
{
    public class StudentInfoDto
    {
        public string StudentNumber { get; set; } = null!;
        public int DepartmentId { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }
}
