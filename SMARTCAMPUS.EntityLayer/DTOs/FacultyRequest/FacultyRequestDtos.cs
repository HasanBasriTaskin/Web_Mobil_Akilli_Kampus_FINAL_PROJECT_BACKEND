namespace SMARTCAMPUS.EntityLayer.DTOs.FacultyRequest
{
    /// <summary>
    /// Akademisyen ders alma isteği oluşturma DTO
    /// </summary>
    public class CreateFacultyRequestDto
    {
        public int SectionId { get; set; }
    }

    /// <summary>
    /// Akademisyen ders alma isteği listeleme DTO
    /// </summary>
    public class FacultyRequestDto
    {
        public int Id { get; set; }
        public int FacultyId { get; set; }
        public string FacultyName { get; set; } = string.Empty;
        public string FacultyTitle { get; set; } = string.Empty;
        public string FacultyEmail { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SectionNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? ResponseDate { get; set; }
        public string? AdminNote { get; set; }
    }

    /// <summary>
    /// Admin onay/red DTO
    /// </summary>
    public class ProcessFacultyRequestDto
    {
        public string? Note { get; set; }
    }

    /// <summary>
    /// Bölümdeki uygun dersler listesi DTO
    /// </summary>
    public class AvailableSectionDto
    {
        public int SectionId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SectionNumber { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Capacity { get; set; }
        public bool AlreadyRequested { get; set; }
        public bool AlreadyAssigned { get; set; }
    }
}
