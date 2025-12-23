using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface ICafeteriaDal : IGenericDal<Cafeteria>
    {
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<bool> HasActiveMenusAsync(int cafeteriaId);
        Task<bool> HasActiveReservationsAsync(int cafeteriaId);
    }
}
