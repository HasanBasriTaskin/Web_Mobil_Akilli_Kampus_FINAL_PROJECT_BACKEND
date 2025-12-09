using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.User;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly CampusContext _context;

        // Note: Using 'UserManager' here refers to Identity's UserManager
        public UserService(UserManager<User> userManager, IMapper mapper, CampusContext context)
        {
            _userManager = userManager;
            _mapper = mapper;
            _context = context;
        }

        public async Task<Response<PagedResponse<UserListDto>>> GetUsersAsync(UserQueryParameters queryParams)
        {
            // Base query
            var usersQuery = _context.Users.AsQueryable();

            // Search filter (name veya email)
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
                // Student veya Faculty departmanına göre filtrele
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

            var pagedResponse = new PagedResponse<UserListDto>(userDtos, queryParams.Page, queryParams.Limit, totalRecords);
            return Response<PagedResponse<UserListDto>>.Success(pagedResponse, 200);
        }

        public async Task<Response<UserProfileDto>> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Response<UserProfileDto>.Fail("Kullanıcı bulunamadı", 404);

            var dto = _mapper.Map<UserProfileDto>(user);
            return Response<UserProfileDto>.Success(dto, 200);
        }

        public async Task<Response<NoDataDto>> UpdateUserAsync(string userId, UserUpdateDto userUpdateDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Response<NoDataDto>.Fail("Kullanıcı bulunamadı", 404);

            user.FullName = userUpdateDto.FullName;
            user.Email = userUpdateDto.Email;
            user.UserName = userUpdateDto.Email;
            user.PhoneNumber = userUpdateDto.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Response<NoDataDto>.Fail(result.Errors.Select(e => e.Description).ToList(), 400);
            }

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Response<NoDataDto>.Fail("Kullanıcı bulunamadı", 404);

            // Soft Delete: Mark as inactive
            user.IsActive = false;
            
            // Invalidate all refresh tokens for this user
            var activeTokens = await _context.RefreshTokens.Where(x => x.UserId == userId && x.Revoked == null).ToListAsync();
            foreach (var token in activeTokens)
            {
                token.Revoked = DateTime.UtcNow;
                token.ReasonRevoked = "User deleted (Soft Delete)";
            }
            
            // Persist token changes
            await _context.SaveChangesAsync();
            
            await _context.SaveChangesAsync();
            
            var result = await _userManager.UpdateAsync(user);
             if (!result.Succeeded)
            {
                return Response<NoDataDto>.Fail(result.Errors.Select(e => e.Description).ToList(), 400);
            }

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> AssignRolesAsync(string userId, List<string> roles)
        {
             var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Response<NoDataDto>.Fail("Kullanıcı bulunamadı", 404);

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return Response<NoDataDto>.Fail(removeResult.Errors.Select(e => e.Description).ToList(), 400);

            var addResult = await _userManager.AddToRolesAsync(user, roles);
            if (!addResult.Succeeded) return Response<NoDataDto>.Fail(addResult.Errors.Select(e => e.Description).ToList(), 400);

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<string>> UploadProfilePictureAsync(string userId, IFormFile file)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Response<string>.Fail("Kullanıcı bulunamadı", 404);

            if (file == null || file.Length == 0)
                return Response<string>.Fail("Dosya boş", 400);

            // 5MB limit kontrolü
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxFileSize)
                return Response<string>.Fail("Dosya boyutu en fazla 5MB olabilir", 400);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return Response<string>.Fail("Geçersiz dosya tipi", 400);

            // Ensure directory exists
            var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile-pictures");
            if (!System.IO.Directory.Exists(uploadsFolder))
                System.IO.Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/profile-pictures/{uniqueFileName}";
            user.ProfilePictureUrl = fileUrl;
            
            await _userManager.UpdateAsync(user);

            return Response<string>.Success(fileUrl, 200);
        }
    }
}
