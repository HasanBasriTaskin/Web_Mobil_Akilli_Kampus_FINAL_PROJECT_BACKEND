using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.User;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IUserService
    {
        Task<Response<PagedResponse<UserListDto>>> GetUsersAsync(UserQueryParameters queryParams);
        Task<Response<UserProfileDto>> GetUserByIdAsync(string userId);
        Task<Response<NoDataDto>> UpdateUserAsync(string userId, UserUpdateDto userUpdateDto);
        Task<Response<NoDataDto>> DeleteUserAsync(string userId);
        Task<Response<NoDataDto>> AssignRolesAsync(string userId,List<string> roles);
        Task<Response<string>> UploadProfilePictureAsync(string userId, Microsoft.AspNetCore.Http.IFormFile file);
    }
}
