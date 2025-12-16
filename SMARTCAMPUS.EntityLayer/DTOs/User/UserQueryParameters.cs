namespace SMARTCAMPUS.EntityLayer.DTOs.User
{
    public class UserQueryParameters
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int Page { get; set; } = 1;

        public int Limit
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        // Filtering
        public string? Role { get; set; }
        public int? DepartmentId { get; set; }

        // Search (name veya email içinde arar)
        public string? Search { get; set; }
    }
}
