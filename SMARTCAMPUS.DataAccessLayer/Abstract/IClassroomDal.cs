using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IClassroomDal : IGenericDal<Classroom>
    {
        Task<Classroom?> GetClassroomByBuildingAndRoomAsync(string building, string roomNumber);
        Task<IEnumerable<Classroom>> GetAvailableClassroomsAsync(DateTime date, TimeSpan startTime, TimeSpan endTime);
    }
}

