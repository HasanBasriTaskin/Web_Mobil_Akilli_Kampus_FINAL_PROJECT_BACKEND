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
        private readonly SMARTCAMPUS.DataAccessLayer.Context.CampusContext _context;

        // Note: Using 'UserManager' here refers to Identity's UserManager
        public UserService(UserManager<User> userManager, IMapper mapper, SMARTCAMPUS.DataAccessLayer.Context.CampusContext context)
        {
            _userManager = userManager;
            _mapper = mapper;
            _context = context;
        }

        public async Task<PagedResponse<UserListDto>> GetUsersAsync(int pageNumber, int pageSize)
        {
            // Optimized Query: Fetch User + Roles in single roundtrip (or optimized batches)
            var query = from u in _context.Users
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
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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
            
            // Invalidate all refresh tokens for this user
            var activeTokens = await _context.RefreshTokens.Where(x => x.UserId == userId && x.Revoked == null).ToListAsync();
            foreach (var token in activeTokens)
            {
                token.Revoked = DateTime.UtcNow;
                token.ReasonRevoked = "User deleted (Soft Delete)";
                // token.RevokedByIp = ... // We don't have IP here easily unless injected HttpContext, skipping for now or adding later
            }
            // _context.UpdateRange(activeTokens); // Not needed if tracking is on, but good practice. 
            // Actually UserManager Update will save User, but tokens are separate dbset in context. 
            // We need to save context. 
            // _userManager shares context? Yes usually. 
            // But let's be explicit. If _userManager.UpdateAsync saves changes, it might only save User.
            // Safest way: _context.SaveChanges() or _context.SaveChangesAsync()
            
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
