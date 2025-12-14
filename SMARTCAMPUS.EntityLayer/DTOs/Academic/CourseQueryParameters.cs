namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class CourseQueryParameters
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string? Search { get; set; } // Search in code, name, description
        public int? DepartmentId { get; set; }
        public int? MinCredits { get; set; }
        public int? MaxCredits { get; set; }
        public string? SortBy { get; set; } // "code", "name", "credits"
        public string? SortOrder { get; set; } // "asc", "desc"
    }
}

