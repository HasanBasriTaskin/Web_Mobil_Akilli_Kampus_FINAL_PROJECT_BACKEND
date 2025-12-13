using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface ICourseSectionDal : IGenericDal<CourseSection>
    {
        Task<CourseSection?> GetSectionWithDetailsAsync(int sectionId);
        Task<IEnumerable<CourseSection>> GetSectionsByCourseAsync(int courseId);
        Task<IEnumerable<CourseSection>> GetSectionsBySemesterAsync(string semester, int year);
        Task<IEnumerable<CourseSection>> GetSectionsByInstructorAsync(string instructorId);
        Task<bool> HasScheduleConflictAsync(int studentId, int sectionId, string semester, int year);
    }
}

