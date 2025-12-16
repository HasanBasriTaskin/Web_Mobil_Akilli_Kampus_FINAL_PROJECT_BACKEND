using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IClassroomDal : IGenericDal<Classroom>
    {
        Task<Classroom?> GetByBuildingAndRoomAsync(string building, string roomNumber);
    }
}
