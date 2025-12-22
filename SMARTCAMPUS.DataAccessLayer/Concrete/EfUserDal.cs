using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.User;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfUserDal : IUserDal
    {
        private readonly CampusContext _context;

        public EfUserDal(CampusContext context)
        {
            _context = context;
        }

        public async Task<(List<UserListDto> Users, int TotalCount)> GetUsersWithRolesAsync(UserQueryParameters queryParams)
        {
            // Base query
            var usersQuery = _context.Users.AsQueryable();

            // Search filter (name or email)
            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var searchLower = queryParams.Search.ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.FullName.ToLower().Contains(searchLower) ||
                    u.Email!.ToLower().Contains(searchLower));
            }

            // Department filter
            if (queryParams.DepartmentId.HasValue)
            {
                var departmentId = queryParams.DepartmentId.Value;
                // Student or Faculty department filtering
                var studentUserIds = _context.Students
                    .Where(s => s.DepartmentId == departmentId)
                    .Select(s => s.UserId);
                var facultyUserIds = _context.Faculties
                    .Where(f => f.DepartmentId == departmentId)
                    .Select(f => f.UserId);

                usersQuery = usersQuery.Where(u =>
                    studentUserIds.Contains(u.Id) || facultyUserIds.Contains(u.Id));
            }

            // Role filter
            if (!string.IsNullOrWhiteSpace(queryParams.Role))
            {
                var roleId = await _context.Roles
                    .Where(r => r.Name == queryParams.Role)
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(roleId))
                {
                    var userIdsWithRole = _context.UserRoles
                        .Where(ur => ur.RoleId == roleId)
                        .Select(ur => ur.UserId);

                    usersQuery = usersQuery.Where(u => userIdsWithRole.Contains(u.Id));
                }
            }

            // Projection with roles
            var query = from u in usersQuery
                        select new UserListDto
                        {
                            Id = u.Id,
                            FullName = u.FullName,
                            Email = u.Email!,
                            PhoneNumber = u.PhoneNumber,
                            IsActive = u.IsActive,
                            Roles = (from ur in _context.UserRoles
                                     join r in _context.Roles on ur.RoleId equals r.Id
                                     where ur.UserId == u.Id
                                     select r.Name!).ToList()
                        };

            var totalRecords = await query.CountAsync();

            var userDtos = await query
                .Skip((queryParams.Page - 1) * queryParams.Limit)
                .Take(queryParams.Limit)
                .ToListAsync();

            return (userDtos, totalRecords);
        }
    }
}
