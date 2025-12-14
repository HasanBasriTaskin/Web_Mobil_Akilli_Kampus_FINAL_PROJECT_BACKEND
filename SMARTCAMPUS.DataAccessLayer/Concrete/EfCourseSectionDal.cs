using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;
using System.Text.Json;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfCourseSectionDal : GenericRepository<CourseSection>, ICourseSectionDal
    {
        public EfCourseSectionDal(CampusContext context) : base(context)
        {
        }

        public async Task<CourseSection?> GetSectionWithDetailsAsync(int sectionId)
        {
            return await _context.CourseSections
                .Include(s => s.Course)
                    .ThenInclude(c => c.Department)
                .Include(s => s.Instructor)
                .Include(s => s.Classroom)
                .FirstOrDefaultAsync(s => s.Id == sectionId);
        }

        public async Task<IEnumerable<CourseSection>> GetSectionsByCourseAsync(int courseId)
        {
            return await _context.CourseSections
                .Where(s => s.CourseId == courseId && s.IsActive)
                .Include(s => s.Course)
                .Include(s => s.Instructor)
                .Include(s => s.Classroom)
                .ToListAsync();
        }

        public async Task<IEnumerable<CourseSection>> GetSectionsBySemesterAsync(string semester, int year)
        {
            return await _context.CourseSections
                .Where(s => s.Semester == semester && s.Year == year && s.IsActive)
                .Include(s => s.Course)
                .Include(s => s.Instructor)
                .Include(s => s.Classroom)
                .ToListAsync();
        }

        public async Task<IEnumerable<CourseSection>> GetSectionsByInstructorAsync(string instructorId)
        {
            return await _context.CourseSections
                .Where(s => s.InstructorId == instructorId && s.IsActive)
                .Include(s => s.Course)
                .Include(s => s.Classroom)
                .ToListAsync();
        }

        public async Task<bool> HasScheduleConflictAsync(int studentId, int sectionId, string semester, int year)
        {
            var newSection = await GetSectionWithDetailsAsync(sectionId);
            if (newSection == null || string.IsNullOrEmpty(newSection.ScheduleJson))
                return false;

            var studentEnrollments = await _context.Enrollments
                .Where(e => e.StudentId == studentId 
                    && e.Status == "Active"
                    && e.Section.Semester == semester
                    && e.Section.Year == year
                    && e.IsActive)
                .Include(e => e.Section)
                .ToListAsync();

            if (!studentEnrollments.Any())
                return false;

            try
            {
                var newSchedule = JsonSerializer.Deserialize<List<ScheduleItem>>(newSection.ScheduleJson);
                if (newSchedule == null || !newSchedule.Any())
                    return false;

                foreach (var enrollment in studentEnrollments)
                {
                    if (string.IsNullOrEmpty(enrollment.Section.ScheduleJson))
                        continue;

                    var existingSchedule = JsonSerializer.Deserialize<List<ScheduleItem>>(enrollment.Section.ScheduleJson);
                    if (existingSchedule == null || !existingSchedule.Any())
                        continue;

                    foreach (var newItem in newSchedule)
                    {
                        foreach (var existingItem in existingSchedule)
                        {
                            if (newItem.Day == existingItem.Day)
                            {
                                if (TimeOverlaps(newItem.StartTime, newItem.EndTime, existingItem.StartTime, existingItem.EndTime))
                                    return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private bool TimeOverlaps(string start1, string end1, string start2, string end2)
        {
            var time1Start = TimeSpan.Parse(start1);
            var time1End = TimeSpan.Parse(end1);
            var time2Start = TimeSpan.Parse(start2);
            var time2End = TimeSpan.Parse(end2);

            return time1Start < time2End && time2Start < time1End;
        }

        private class ScheduleItem
        {
            public string Day { get; set; } = null!;
            public string StartTime { get; set; } = null!;
            public string EndTime { get; set; } = null!;
        }
    }
}



