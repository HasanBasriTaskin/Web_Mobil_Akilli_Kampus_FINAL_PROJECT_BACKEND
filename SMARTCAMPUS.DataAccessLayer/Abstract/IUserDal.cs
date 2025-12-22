using SMARTCAMPUS.EntityLayer.DTOs.User;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IUserDal
    {
        Task<(List<UserListDto> Users, int TotalCount)> GetUsersWithRolesAsync(UserQueryParameters queryParams);
    }
}
