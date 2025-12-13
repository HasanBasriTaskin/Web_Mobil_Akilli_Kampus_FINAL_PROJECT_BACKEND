using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using System.Linq;
using System.Security.Claims;

namespace SMARTCAMPUS.BusinessLayer.Tools
{
    public class UserClaimsHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public UserClaimsHelper(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Gets the current user's ID from JWT token claims
        /// </summary>
        public string? GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

            return userId;
        }

        /// <summary>
        /// Gets the current user's email from JWT token claims
        /// </summary>
        public string? GetUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;
        }

        /// <summary>
        /// Gets the current student's ID from JWT token (requires database lookup)
        /// </summary>
        public async Task<int?> GetStudentIdAsync()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return null;

            var students = _unitOfWork.Students.Where(s => s.UserId == userId && s.IsActive);
            var student = await students.FirstOrDefaultAsync();

            return student?.Id;
        }

        /// <summary>
        /// Gets the current faculty's ID from JWT token (requires database lookup)
        /// </summary>
        public async Task<int?> GetFacultyIdAsync()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return null;

            var faculties = _unitOfWork.Faculties.Where(f => f.UserId == userId && f.IsActive);
            var faculty = await faculties.FirstOrDefaultAsync();

            return faculty?.Id;
        }

        /// <summary>
        /// Checks if the current user has a specific role
        /// </summary>
        public bool IsInRole(string role)
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
        }

        /// <summary>
        /// Gets all roles of the current user
        /// </summary>
        public IEnumerable<string> GetRoles()
        {
            return _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value) ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets student with details (User and Department) by student ID
        /// </summary>
        public async Task<EntityLayer.Models.Student?> GetStudentWithDetailsAsync(int studentId)
        {
            return await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);
        }
    }
}

