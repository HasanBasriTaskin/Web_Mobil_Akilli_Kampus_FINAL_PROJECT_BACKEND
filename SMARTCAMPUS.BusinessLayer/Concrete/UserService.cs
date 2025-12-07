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

        // Note: Using 'UserManager' here refers to Identity's UserManager
        public UserService(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<PagedResponse<UserListDto>> GetUsersAsync(int pageNumber, int pageSize)
        {
            var query = _userManager.Users.AsQueryable();

            var totalRecords = await query.CountAsync();
            
            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userDtos = new List<UserListDto>();

            // Need to fetch roles for each user? 
            // Warning: Loop inside async might be N+1 problem if not careful. 
            // Identity doesn't eager load roles by default in a simple way for Lists.
            // For performance, we might want to skip roles in lists or use a joined query.
            // Let's settle for basic mapping for now, assuming Roles are not strict requirement for List view, 
            // OR we fetch them. 
            
            foreach (var user in users)
            {
                var dto = _mapper.Map<UserListDto>(user);
                dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                userDtos.Add(dto);
            }

            // TODO: Paginate Response needs to be simpler or PagedResponse constructor needs to handle it.
            return new PagedResponse<UserListDto>(userDtos, pageNumber, pageSize, totalRecords);
        }

        public async Task<Response<UserProfileDto>> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Response<UserProfileDto>.Fail("User not found", 404);

            var dto = _mapper.Map<UserProfileDto>(user);
            return Response<UserProfileDto>.Success(dto, 200);
        }

        public async Task<Response<NoDataDto>> UpdateUserAsync(string userId, UserUpdateDto userUpdateDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Response<NoDataDto>.Fail("User not found", 404);

            user.FullName = userUpdateDto.FullName;
            user.Email = userUpdateDto.Email;
            user.UserName = userUpdateDto.Email; // Keep UserName synced with Email if that's the policy
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
            if (user == null) return Response<NoDataDto>.Fail("User not found", 404);

            // Soft delete logic is usually preferred (IsActive = false).
            // But if requested "Delete", we can do Hard Delete or Soft Delete.
            // Let's assume Hard Delete for now unless specified otherwise, OR check IsActive property.
            // User entity has IsActive. Let's do Soft Delete!
            
            // Wait, standard Identity Delete causes row removal. 
            // If we want Soft Delete, we Update IsActive = false.
            
            // "DeleteUser" usually implies removal. Let's do HARD delete for initial implementation 
            // but be aware of foreign key constraints (Students, RefreshTokens etc).
            // Actually, Soft Delete is safer.
            
            user.IsActive = false;
            // logic to invalidate tokens?
            
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
            if (user == null) return Response<NoDataDto>.Fail("User not found", 404);

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return Response<NoDataDto>.Fail(removeResult.Errors.Select(e => e.Description).ToList(), 400);

            var addResult = await _userManager.AddToRolesAsync(user, roles);
            if (!addResult.Succeeded) return Response<NoDataDto>.Fail(addResult.Errors.Select(e => e.Description).ToList(), 400);

            return Response<NoDataDto>.Success(200);
        }
    }
}
