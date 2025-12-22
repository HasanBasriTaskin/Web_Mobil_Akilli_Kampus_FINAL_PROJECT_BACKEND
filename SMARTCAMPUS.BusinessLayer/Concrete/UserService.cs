using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;
using SMARTCAMPUS.EntityLayer.DTOs.User;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        // Note: Using 'UserManager' here refers to Identity's UserManager
        public UserService(UserManager<User> userManager, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<PagedResponse<UserListDto>>> GetUsersAsync(UserQueryParameters queryParams)
        {
            var (users, totalRecords) = await _unitOfWork.Users.GetUsersWithRolesAsync(queryParams);
            var pagedResponse = new PagedResponse<UserListDto>(users, queryParams.Page, queryParams.Limit, totalRecords);
            return Response<PagedResponse<UserListDto>>.Success(pagedResponse, 200);
        }

        public async Task<Response<UserDto>> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Response<UserDto>.Fail("Kullanıcı bulunamadı", 404);

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "User";

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                UserType = primaryRole,
                Role = primaryRole,
                IsEmailVerified = user.EmailConfirmed,
                IsActive = user.IsActive,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CreatedAt = user.CreatedDate,
                Roles = roles
            };

            // Fetch Student or Faculty info based on role
            if (primaryRole == "Student")
            {
                var student = await _unitOfWork.Students.GetByUserIdAsync(user.Id);
                if (student != null)
                {
                    userDto.Student = new StudentInfoDto
                    {
                        StudentNumber = student.StudentNumber,
                        DepartmentId = student.DepartmentId,
                        EnrollmentDate = student.CreatedDate
                    };
                }
            }
            else if (primaryRole == "Faculty")
            {
                var faculty = await _unitOfWork.Faculties.GetByUserIdAsync(user.Id);
                if (faculty != null)
                {
                    userDto.Faculty = new FacultyInfoDto
                    {
                        EmployeeNumber = faculty.EmployeeNumber,
                        Title = faculty.Title,
                        DepartmentId = faculty.DepartmentId,
                        OfficeLocation = faculty.OfficeLocation
                    };
                }
            }

            return Response<UserDto>.Success(userDto, 200);
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
            // We need to implement a Revoke method in DAL if not exists, OR use generic Update loop.
            // But DAL usually has access to context.
            // Let's see what RefreshTokenDal has. It likely has GetByUserId.
            // Since we moved to UoW, let's use the UoW repositories.
            
            var tokens = await _unitOfWork.RefreshTokens.Where(x => x.UserId == userId && x.Revoked == null).ToListAsync();
            foreach (var token in tokens)
            {
                token.Revoked = DateTime.UtcNow;
                token.ReasonRevoked = "User deleted (Soft Delete)";
                _unitOfWork.RefreshTokens.Update(token);
            }
            
            await _unitOfWork.CommitAsync();
            
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
