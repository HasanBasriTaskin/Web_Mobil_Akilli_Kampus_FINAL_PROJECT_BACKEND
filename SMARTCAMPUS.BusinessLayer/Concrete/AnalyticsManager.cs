using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs.Analytics;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    /// <summary>
    /// Analitik ve raporlama servisi implementasyonu
    /// </summary>
    public class AnalyticsManager : IAnalyticsService
    {
        private readonly CampusContext _context;

        public AnalyticsManager(CampusContext context)
        {
            _context = context;
        }

        public async Task<Response<AdminDashboardDto>> GetDashboardStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalStudents = await _context.Students.CountAsync(s => s.IsActive);
            var totalFaculty = await _context.Faculties.CountAsync(f => f.IsActive);
            var totalCourses = await _context.Courses.CountAsync(c => c.IsActive);
            var totalSections = await _context.CourseSections.CountAsync(cs => cs.IsActive);
            var activeEnrollments = await _context.Enrollments
                .CountAsync(e => e.IsActive && e.Status == EnrollmentStatus.Enrolled);
            var totalEvents = await _context.Events.CountAsync(e => e.IsActive);
            var upcomingEvents = await _context.Events
                .CountAsync(e => e.IsActive && e.StartDate > DateTime.UtcNow);

            // Ders doluluk oranı hesaplama
            var occupancyData = await _context.CourseSections
                .Where(cs => cs.IsActive && cs.Capacity > 0)
                .Select(cs => new
                {
                    cs.Capacity,
                    EnrolledCount = cs.Enrollments.Count(e => e.IsActive && e.Status == EnrollmentStatus.Enrolled)
                })
                .ToListAsync();

            var avgOccupancy = occupancyData.Count > 0
                ? occupancyData.Average(x => (double)x.EnrolledCount / x.Capacity * 100)
                : 0;

            var dashboard = new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                TotalStudents = totalStudents,
                TotalFaculty = totalFaculty,
                DailyActiveUsers = totalUsers, // TODO: Gerçek DAU için login tracking gerekli
                TotalCourses = totalCourses,
                TotalCourseSections = totalSections,
                ActiveEnrollments = activeEnrollments,
                AverageClassOccupancy = Math.Round(avgOccupancy, 2),
                TotalEvents = totalEvents,
                UpcomingEvents = upcomingEvents,
                SystemHealth = new SystemHealthDto
                {
                    DatabaseStatus = "Healthy",
                    ApiStatus = "Running",
                    LastChecked = DateTime.UtcNow,
                    ActiveConnections = 0,
                    CpuUsagePercent = 0,
                    MemoryUsagePercent = 0
                }
            };

            return Response<AdminDashboardDto>.Success(dashboard, 200);
        }

        public async Task<Response<AcademicPerformanceDto>> GetAcademicPerformanceAsync()
        {
            var departmentStats = await GetDepartmentGpaStatsInternalAsync();
            var gradeDistribution = await GetGradeDistributionInternalAsync(null);

            var enrollments = await _context.Enrollments
                .Where(e => e.IsActive && e.LetterGrade != null)
                .ToListAsync();

            var passedGrades = new[] { "AA", "BA", "BB", "CB", "CC", "DC", "DD" };
            var passedCount = enrollments.Count(e => passedGrades.Contains(e.LetterGrade));
            var failedCount = enrollments.Count(e => e.LetterGrade == "FF" || e.LetterGrade == "FD");

            var performance = new AcademicPerformanceDto
            {
                OverallAverageGpa = departmentStats.Count > 0
                    ? Math.Round(departmentStats.Average(d => d.AverageGpa), 2)
                    : 0,
                TotalEnrollments = enrollments.Count,
                PassedCount = passedCount,
                FailedCount = failedCount,
                PassRate = enrollments.Count > 0
                    ? Math.Round((double)passedCount / enrollments.Count * 100, 2)
                    : 0,
                DepartmentStats = departmentStats,
                GradeDistribution = gradeDistribution
            };

            return Response<AcademicPerformanceDto>.Success(performance, 200);
        }

        public async Task<Response<List<DepartmentGpaDto>>> GetDepartmentGpaStatsAsync()
        {
            var stats = await GetDepartmentGpaStatsInternalAsync();
            return Response<List<DepartmentGpaDto>>.Success(stats, 200);
        }

        public async Task<Response<DepartmentGpaDto>> GetDepartmentStatsAsync(int departmentId)
        {
            var department = await _context.Departments
                .Where(d => d.Id == departmentId && d.IsActive)
                .FirstOrDefaultAsync();

            if (department == null)
            {
                return Response<DepartmentGpaDto>.Fail("Bölüm bulunamadı", 404);
            }

            var students = await _context.Students
                .Where(s => s.DepartmentId == departmentId && s.IsActive)
                .ToListAsync();

            var dto = new DepartmentGpaDto
            {
                DepartmentId = department.Id,
                DepartmentName = department.Name,
                DepartmentCode = department.Code,
                AverageGpa = students.Count > 0 ? Math.Round(students.Average(s => s.GPA), 2) : 0,
                AverageCgpa = students.Count > 0 ? Math.Round(students.Average(s => s.CGPA), 2) : 0,
                StudentCount = students.Count
            };

            return Response<DepartmentGpaDto>.Success(dto, 200);
        }

        public async Task<Response<List<GradeDistributionDto>>> GetGradeDistributionAsync(int? sectionId = null)
        {
            var distribution = await GetGradeDistributionInternalAsync(sectionId);
            return Response<List<GradeDistributionDto>>.Success(distribution, 200);
        }

        public async Task<Response<List<AtRiskStudentDto>>> GetAtRiskStudentsAsync(
            double gpaThreshold = 2.0,
            double attendanceThreshold = 20)
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Department)
                .Where(s => s.IsActive && (s.GPA < gpaThreshold || s.CGPA < gpaThreshold))
                .ToListAsync();

            var atRiskStudents = new List<AtRiskStudentDto>();

            foreach (var student in students)
            {
                // Devamsızlık oranını hesapla
                var attendanceRate = await CalculateAttendanceRateAsync(student.Id);
                var riskFactors = new List<string>();
                var riskLevel = RiskLevel.Low;

                if (student.GPA < 1.5)
                {
                    riskFactors.Add("Çok düşük dönem GPA");
                    riskLevel = RiskLevel.Critical;
                }
                else if (student.GPA < 2.0)
                {
                    riskFactors.Add("Düşük dönem GPA");
                    riskLevel = RiskLevel.High;
                }

                if (student.CGPA < 2.0)
                {
                    riskFactors.Add("Düşük genel GPA");
                    if (riskLevel < RiskLevel.High)
                        riskLevel = RiskLevel.Medium;
                }

                if (attendanceRate < (100 - attendanceThreshold))
                {
                    riskFactors.Add($"Yüksek devamsızlık (%{100 - attendanceRate:F0})");
                    if (riskLevel < RiskLevel.Medium)
                        riskLevel = RiskLevel.Medium;
                }

                atRiskStudents.Add(new AtRiskStudentDto
                {
                    StudentId = student.Id,
                    StudentNumber = student.StudentNumber,
                    FullName = student.User?.FullName ?? "",
                    Email = student.User?.Email ?? "",
                    DepartmentName = student.Department?.Name ?? "",
                    Gpa = student.GPA,
                    Cgpa = student.CGPA,
                    AttendanceRate = attendanceRate,
                    RiskLevel = riskLevel.ToString(),
                    RiskFactors = riskFactors
                });
            }

            return Response<List<AtRiskStudentDto>>.Success(
                atRiskStudents.OrderByDescending(s => GetRiskLevelValue(s.RiskLevel)).ToList(),
                200);
        }

        public async Task<Response<List<CourseOccupancyDto>>> GetCourseOccupancyAsync()
        {
            var sections = await _context.CourseSections
                .Include(cs => cs.Course)
                .Where(cs => cs.IsActive && cs.Capacity > 0)
                .Select(cs => new CourseOccupancyDto
                {
                    SectionId = cs.Id,
                    CourseCode = cs.Course!.Code,
                    CourseName = cs.Course.Name,
                    SectionCode = cs.SectionNumber,
                    Capacity = cs.Capacity,
                    EnrolledCount = cs.Enrollments.Count(e => e.IsActive && e.Status == EnrollmentStatus.Enrolled),
                    OccupancyRate = 0
                })
                .ToListAsync();

            foreach (var section in sections)
            {
                section.OccupancyRate = Math.Round((double)section.EnrolledCount / section.Capacity * 100, 2);
            }

            return Response<List<CourseOccupancyDto>>.Success(
                sections.OrderByDescending(s => s.OccupancyRate).ToList(),
                200);
        }

        public async Task<Response<AttendanceStatsDto>> GetAttendanceStatsAsync()
        {
            var totalSessions = await _context.AttendanceSessions.CountAsync(a => a.IsActive);
            var totalRecords = await _context.AttendanceRecords.CountAsync(a => a.IsActive);

            // AttendanceRecord varlığı = katılım (IsFlagged = sorunlu katılım)
            var validRecords = await _context.AttendanceRecords
                .CountAsync(a => a.IsActive && !a.IsFlagged);

            var overallRate = totalRecords > 0
                ? Math.Round((double)validRecords / totalRecords * 100, 2)
                : 100;

            // %80'den düşük katılıma sahip öğrenci sayısı
            var studentsAboveThreshold = await CountStudentsWithLowAttendanceAsync(20);

            var sectionStats = await _context.CourseSections
                .Include(cs => cs.Course)
                .Where(cs => cs.IsActive)
                .Select(cs => new SectionAttendanceDto
                {
                    SectionId = cs.Id,
                    CourseCode = cs.Course!.Code,
                    CourseName = cs.Course.Name,
                    TotalSessions = cs.AttendanceSessions.Count(a => a.IsActive),
                    AverageAttendanceRate = 0
                })
                .ToListAsync();

            // Her section için ortalama devamsızlık hesapla
            foreach (var section in sectionStats)
            {
                if (section.TotalSessions > 0)
                {
                    var sectionRecords = await _context.AttendanceRecords
                        .Include(ar => ar.Session)
                        .Where(ar => ar.Session.SectionId == section.SectionId && ar.IsActive)
                        .ToListAsync();

                    // Katılım kaydı olan = katılmış, IsFlagged değilse geçerli katılım
                    var validInSection = sectionRecords.Count(r => !r.IsFlagged);
                    section.AverageAttendanceRate = sectionRecords.Count > 0
                        ? Math.Round((double)validInSection / sectionRecords.Count * 100, 2)
                        : 100;
                }
            }

            var stats = new AttendanceStatsDto
            {
                TotalSessions = totalSessions,
                TotalAttendanceRecords = totalRecords,
                OverallAttendanceRate = overallRate,
                StudentsAboveThreshold = studentsAboveThreshold,
                SectionStats = sectionStats.OrderBy(s => s.AverageAttendanceRate).ToList()
            };

            return Response<AttendanceStatsDto>.Success(stats, 200);
        }

        #region Private Helper Methods

        private async Task<List<DepartmentGpaDto>> GetDepartmentGpaStatsInternalAsync()
        {
            return await _context.Departments
                .Where(d => d.IsActive)
                .Select(d => new DepartmentGpaDto
                {
                    DepartmentId = d.Id,
                    DepartmentName = d.Name,
                    DepartmentCode = d.Code,
                    AverageGpa = d.Students != null && d.Students.Where(s => s.IsActive).Any()
                        ? Math.Round(d.Students.Where(s => s.IsActive).Average(s => s.GPA), 2)
                        : 0,
                    AverageCgpa = d.Students != null && d.Students.Where(s => s.IsActive).Any()
                        ? Math.Round(d.Students.Where(s => s.IsActive).Average(s => s.CGPA), 2)
                        : 0,
                    StudentCount = d.Students != null ? d.Students.Count(s => s.IsActive) : 0
                })
                .OrderByDescending(d => d.AverageGpa)
                .ToListAsync();
        }

        private async Task<List<GradeDistributionDto>> GetGradeDistributionInternalAsync(int? sectionId)
        {
            var query = _context.Enrollments.Where(e => e.IsActive && e.LetterGrade != null);

            if (sectionId.HasValue)
            {
                query = query.Where(e => e.SectionId == sectionId.Value);
            }

            var grades = await query
                .GroupBy(e => e.LetterGrade)
                .Select(g => new { Grade = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalCount = grades.Sum(g => g.Count);

            return grades
                .Select(g => new GradeDistributionDto
                {
                    LetterGrade = g.Grade ?? "N/A",
                    Count = g.Count,
                    Percentage = totalCount > 0 ? Math.Round((double)g.Count / totalCount * 100, 2) : 0
                })
                .OrderBy(g => GetGradeOrder(g.LetterGrade))
                .ToList();
        }

        private async Task<double> CalculateAttendanceRateAsync(int studentId)
        {
            // Öğrencinin tüm yoklama kayıtlarını al
            var records = await _context.AttendanceRecords
                .Where(ar => ar.StudentId == studentId && ar.IsActive)
                .ToListAsync();

            if (records.Count == 0) return 100; // Kayıt yoksa %100 kabul et

            // IsFlagged olmayan kayıtlar = geçerli katılım
            var validCount = records.Count(r => !r.IsFlagged);
            return Math.Round((double)validCount / records.Count * 100, 2);
        }

        private async Task<int> CountStudentsWithLowAttendanceAsync(double threshold)
        {
            var students = await _context.Students
                .Where(s => s.IsActive)
                .Select(s => s.Id)
                .ToListAsync();

            var count = 0;
            foreach (var studentId in students)
            {
                var rate = await CalculateAttendanceRateAsync(studentId);
                if (rate < (100 - threshold))
                {
                    count++;
                }
            }
            return count;
        }

        private static int GetGradeOrder(string grade)
        {
            return grade switch
            {
                "AA" => 1,
                "BA" => 2,
                "BB" => 3,
                "CB" => 4,
                "CC" => 5,
                "DC" => 6,
                "DD" => 7,
                "FD" => 8,
                "FF" => 9,
                _ => 10
            };
        }

        private static int GetRiskLevelValue(string riskLevel)
        {
            return riskLevel switch
            {
                "Critical" => 4,
                "High" => 3,
                "Medium" => 2,
                "Low" => 1,
                _ => 0
            };
        }

        #endregion
    }
}
