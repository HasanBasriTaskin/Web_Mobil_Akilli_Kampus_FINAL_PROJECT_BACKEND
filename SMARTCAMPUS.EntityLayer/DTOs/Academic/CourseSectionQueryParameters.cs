namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class CourseSectionQueryParameters
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public int? CourseId { get; set; }
        public string? Semester { get; set; }
        public int? Year { get; set; }
        public string? InstructorId { get; set; }
        public string? Search { get; set; } // Search in course code, course name
        public string? SortBy { get; set; } // "courseCode", "sectionNumber", "semester"
        public string? SortOrder { get; set; } // "asc", "desc"
    }
}

