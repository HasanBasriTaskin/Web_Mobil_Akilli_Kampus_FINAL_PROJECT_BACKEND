using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<CampusContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // 1. Auto Migrate
            await context.Database.MigrateAsync();

            // 2. Helper Method Calls
            await SeedRolesAsync(roleManager);
            await SeedDepartmentsAsync(context, configuration);
            await SeedAdminAsync(userManager, configuration);
            await SeedStudentsAsync(userManager, context, configuration);
            await SeedFacultyAsync(userManager, context, configuration);
            await SeedClassroomsAsync(context);
            await SeedCoursesAsync(context);
            await SeedCoursePrerequisitesAsync(context);
            await SeedCourseSectionsAsync(context, userManager);
            await SeedEnrollmentsAsync(context);
            await SeedAttendanceSessionsAsync(context, userManager);
            await SeedAttendanceRecordsAsync(context);
            await SeedExcuseRequestsAsync(context, userManager);
        }

        private static async Task SeedRolesAsync(RoleManager<Role> roleManager)
        {
            string[] roles = { "Admin", "Student", "Faculty" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role { Name = roleName, Description = $"{roleName} Role" });
                }
            }
        }

        private static async Task SeedAdminAsync(UserManager<User> userManager, IConfiguration configuration)
        {
            var adminEmail = configuration["DefaultAdmin:Email"];
            var adminPassword = configuration["DefaultAdmin:Password"];
            var adminName = configuration["DefaultAdmin:FullName"];

            if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
            {
                var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
                if (existingAdmin == null)
                {
                    var adminUser = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = adminName ?? "Admin",
                        EmailConfirmed = true,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
            }
        }

        private static async Task SeedStudentsAsync(UserManager<User> userManager, CampusContext context, IConfiguration configuration)
        {
            var studentsSection = configuration.GetSection("SeedData:Students");
            var students = studentsSection.GetChildren();

            foreach (var studentConf in students)
            {
                var email = studentConf["Email"];
                var existingUser = await userManager.FindByEmailAsync(email);
                if (existingUser == null)
                {
                    var user = new User
                    {
                        UserName = email,
                        Email = email,
                        FullName = studentConf["FullName"],
                        EmailConfirmed = true,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    };

                    var result = await userManager.CreateAsync(user, studentConf["Password"]);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Student");
                        
                        var deptCode = studentConf["DepartmentCode"];
                        var dept = await context.Departments.FirstOrDefaultAsync(d => d.Code == deptCode);
                        // If null, handle gracefully or use default
                        var deptId = dept?.Id ?? 1;

                        var student = new Student
                        {
                            UserId = user.Id,
                            StudentNumber = studentConf["StudentNumber"],
                            DepartmentId = deptId
                        };
                        await context.Students.AddAsync(student);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        private static async Task SeedFacultyAsync(UserManager<User> userManager, CampusContext context, IConfiguration configuration)
        {
            var facultySection = configuration.GetSection("SeedData:Faculty"); // Config keys are case-insensitive usually
            var faculties = facultySection.GetChildren();

            foreach (var facultyConf in faculties)
            {
                var email = facultyConf["Email"];
                var existingUser = await userManager.FindByEmailAsync(email);
                if (existingUser == null)
                {
                    var user = new User
                    {
                        UserName = email,
                        Email = email,
                        FullName = facultyConf["FullName"],
                        EmailConfirmed = true,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    };

                    var result = await userManager.CreateAsync(user, facultyConf["Password"]);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Faculty");

                        var deptCode = facultyConf["DepartmentCode"];
                        var dept = await context.Departments.FirstOrDefaultAsync(d => d.Code == deptCode);
                         var deptId = dept?.Id ?? 1;


                        var faculty = new Faculty
                        {
                            UserId = user.Id,
                            EmployeeNumber = facultyConf["EmployeeNumber"],
                            Title = facultyConf["Title"],
                            OfficeLocation = facultyConf["OfficeLocation"],
                            DepartmentId = deptId
                        };
                        await context.Faculties.AddAsync(faculty);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        private static async Task SeedDepartmentsAsync(CampusContext context, IConfiguration configuration)
        {
            if (!await context.Departments.AnyAsync())
            {
                var departmentsSection = configuration.GetSection("SeedData:Departments");
                var departmentsConf = departmentsSection.GetChildren();
                var departments = new List<Department>();

                foreach (var deptConf in departmentsConf)
                {
                    departments.Add(new Department
                    {
                        Name = deptConf["Name"],
                        Code = deptConf["Code"],
                        FacultyName = deptConf["FacultyName"],
                        Description = deptConf["Description"]
                    });
                }

                if (departments.Any())
                {
                    await context.Departments.AddRangeAsync(departments);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedClassroomsAsync(CampusContext context)
        {
            if (!await context.Classrooms.AnyAsync())
            {
                var classrooms = new List<Classroom>
                {
                    new Classroom { Building = "A", RoomNumber = "101", Capacity = 50, FeaturesJson = "{\"projector\": true, \"computer\": true, \"whiteboard\": true}" },
                    new Classroom { Building = "A", RoomNumber = "102", Capacity = 80, FeaturesJson = "{\"projector\": true, \"computer\": true, \"whiteboard\": true}" },
                    new Classroom { Building = "A", RoomNumber = "201", Capacity = 100, FeaturesJson = "{\"projector\": true, \"computer\": true, \"whiteboard\": true, \"sound\": true}" },
                    new Classroom { Building = "B", RoomNumber = "101", Capacity = 60, FeaturesJson = "{\"projector\": true, \"whiteboard\": true}" },
                    new Classroom { Building = "B", RoomNumber = "205", Capacity = 40, FeaturesJson = "{\"projector\": true, \"computer\": true}" },
                    new Classroom { Building = "Engineering", RoomNumber = "Lab-1", Capacity = 30, FeaturesJson = "{\"projector\": true, \"computer\": true, \"lab\": true}" },
                    new Classroom { Building = "Engineering", RoomNumber = "Lab-2", Capacity = 30, FeaturesJson = "{\"projector\": true, \"computer\": true, \"lab\": true}" }
                };

                await context.Classrooms.AddRangeAsync(classrooms);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedCoursesAsync(CampusContext context)
        {
            if (!await context.Courses.AnyAsync())
            {
                var departments = await context.Departments.ToListAsync();
                var csDept = departments.FirstOrDefault(d => d.Code == "CS") ?? departments.FirstOrDefault();
                var seDept = departments.FirstOrDefault(d => d.Code == "SE") ?? departments.FirstOrDefault();
                var ceDept = departments.FirstOrDefault(d => d.Code == "CE") ?? departments.FirstOrDefault();

                var courses = new List<Course>
                {
                    new Course { Code = "CS101", Name = "Introduction to Computer Science", Description = "Fundamentals of computer science", Credits = 3, ECTS = 5, DepartmentId = csDept?.Id ?? 1 },
                    new Course { Code = "CS102", Name = "Data Structures", Description = "Introduction to data structures and algorithms", Credits = 4, ECTS = 6, DepartmentId = csDept?.Id ?? 1 },
                    new Course { Code = "CS201", Name = "Object-Oriented Programming", Description = "OOP concepts and design patterns", Credits = 4, ECTS = 6, DepartmentId = csDept?.Id ?? 1 },
                    new Course { Code = "CS301", Name = "Database Systems", Description = "Relational database design and SQL", Credits = 3, ECTS = 5, DepartmentId = csDept?.Id ?? 1 },
                    new Course { Code = "SE101", Name = "Software Engineering Fundamentals", Description = "Introduction to software engineering", Credits = 3, ECTS = 5, DepartmentId = seDept?.Id ?? 1 },
                    new Course { Code = "SE201", Name = "Software Design Patterns", Description = "Design patterns in software development", Credits = 4, ECTS = 6, DepartmentId = seDept?.Id ?? 1 },
                    new Course { Code = "CE101", Name = "Introduction to Civil Engineering", Description = "Fundamentals of civil engineering", Credits = 3, ECTS = 5, DepartmentId = ceDept?.Id ?? 1 },
                    new Course { Code = "MATH101", Name = "Calculus I", Description = "Differential and integral calculus", Credits = 4, ECTS = 6, DepartmentId = csDept?.Id ?? 1 },
                    new Course { Code = "MATH102", Name = "Calculus II", Description = "Advanced calculus topics", Credits = 4, ECTS = 6, DepartmentId = csDept?.Id ?? 1 }
                };

                await context.Courses.AddRangeAsync(courses);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedCoursePrerequisitesAsync(CampusContext context)
        {
            if (!await context.CoursePrerequisites.AnyAsync())
            {
                var courses = await context.Courses.ToListAsync();
                var cs101 = courses.FirstOrDefault(c => c.Code == "CS101");
                var cs102 = courses.FirstOrDefault(c => c.Code == "CS102");
                var cs201 = courses.FirstOrDefault(c => c.Code == "CS201");
                var cs301 = courses.FirstOrDefault(c => c.Code == "CS301");
                var math101 = courses.FirstOrDefault(c => c.Code == "MATH101");
                var math102 = courses.FirstOrDefault(c => c.Code == "MATH102");
                var se101 = courses.FirstOrDefault(c => c.Code == "SE101");
                var se201 = courses.FirstOrDefault(c => c.Code == "SE201");

                var prerequisites = new List<CoursePrerequisite>();

                if (cs102 != null && cs101 != null)
                    prerequisites.Add(new CoursePrerequisite { CourseId = cs102.Id, PrerequisiteCourseId = cs101.Id });

                if (cs201 != null && cs102 != null)
                    prerequisites.Add(new CoursePrerequisite { CourseId = cs201.Id, PrerequisiteCourseId = cs102.Id });

                if (cs301 != null && cs201 != null)
                    prerequisites.Add(new CoursePrerequisite { CourseId = cs301.Id, PrerequisiteCourseId = cs201.Id });

                if (math102 != null && math101 != null)
                    prerequisites.Add(new CoursePrerequisite { CourseId = math102.Id, PrerequisiteCourseId = math101.Id });

                if (se201 != null && se101 != null)
                    prerequisites.Add(new CoursePrerequisite { CourseId = se201.Id, PrerequisiteCourseId = se101.Id });

                if (prerequisites.Any())
                {
                    await context.CoursePrerequisites.AddRangeAsync(prerequisites);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedCourseSectionsAsync(CampusContext context, UserManager<User> userManager)
        {
            if (!await context.CourseSections.AnyAsync())
            {
                var courses = await context.Courses.ToListAsync();
                var classrooms = await context.Classrooms.ToListAsync();
                var facultyUsers = await userManager.GetUsersInRoleAsync("Faculty");
                var faculty = facultyUsers.FirstOrDefault();

                var currentYear = DateTime.Now.Year;
                var currentSemester = DateTime.Now.Month >= 9 ? "Fall" : "Spring";

                var sections = new List<CourseSection>();

                foreach (var course in courses.Take(5)) // Seed sections for first 5 courses
                {
                    var classroom = classrooms.FirstOrDefault();
                    var scheduleJson = "[{\"day\": \"Monday\", \"startTime\": \"09:00\", \"endTime\": \"10:30\"}, {\"day\": \"Wednesday\", \"startTime\": \"09:00\", \"endTime\": \"10:30\"}]";

                    sections.Add(new CourseSection
                    {
                        CourseId = course.Id,
                        SectionNumber = "A",
                        Semester = currentSemester,
                        Year = currentYear,
                        InstructorId = faculty?.Id,
                        Capacity = 50,
                        EnrolledCount = 0,
                        ScheduleJson = scheduleJson,
                        ClassroomId = classroom?.Id
                    });
                }

                if (sections.Any())
                {
                    await context.CourseSections.AddRangeAsync(sections);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedEnrollmentsAsync(CampusContext context)
        {
            if (!await context.Enrollments.AnyAsync())
            {
                var students = await context.Students.Take(3).ToListAsync();
                var sections = await context.CourseSections.Take(3).ToListAsync();

                if (!students.Any() || !sections.Any())
                    return;

                var enrollments = new List<Enrollment>();

                // Enroll first student to first 2 sections
                if (students.Count > 0 && sections.Count > 0)
                {
                    enrollments.Add(new Enrollment
                    {
                        StudentId = students[0].Id,
                        SectionId = sections[0].Id,
                        Status = "Active",
                        EnrollmentDate = DateTime.UtcNow.AddDays(-30)
                    });

                    if (sections.Count > 1)
                    {
                        enrollments.Add(new Enrollment
                        {
                            StudentId = students[0].Id,
                            SectionId = sections[1].Id,
                            Status = "Active",
                            EnrollmentDate = DateTime.UtcNow.AddDays(-25)
                        });
                    }
                }

                // Enroll second student to first section
                if (students.Count > 1 && sections.Count > 0)
                {
                    enrollments.Add(new Enrollment
                    {
                        StudentId = students[1].Id,
                        SectionId = sections[0].Id,
                        Status = "Active",
                        EnrollmentDate = DateTime.UtcNow.AddDays(-28)
                    });
                }

                // Enroll third student to second section with grades (completed course)
                if (students.Count > 2 && sections.Count > 1)
                {
                    enrollments.Add(new Enrollment
                    {
                        StudentId = students[2].Id,
                        SectionId = sections[1].Id,
                        Status = "Completed",
                        EnrollmentDate = DateTime.UtcNow.AddMonths(-6),
                        MidtermGrade = 75,
                        FinalGrade = 85,
                        LetterGrade = "B",
                        GradePoint = 3.0m
                    });
                }

                if (enrollments.Any())
                {
                    await context.Enrollments.AddRangeAsync(enrollments);
                    
                    // Update enrolled counts
                    foreach (var enrollment in enrollments.Where(e => e.Status == "Active"))
                    {
                        var section = sections.FirstOrDefault(s => s.Id == enrollment.SectionId);
                        if (section != null)
                        {
                            section.EnrolledCount++;
                        }
                    }
                    
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedAttendanceSessionsAsync(CampusContext context, UserManager<User> userManager)
        {
            if (!await context.AttendanceSessions.AnyAsync())
            {
                var sections = await context.CourseSections.Take(2).ToListAsync();
                var facultyUsers = await userManager.GetUsersInRoleAsync("Faculty");
                var instructor = facultyUsers.FirstOrDefault();

                if (!sections.Any())
                    return;

                var sessions = new List<AttendanceSession>();
                var today = DateTime.UtcNow.Date;

                foreach (var section in sections)
                {
                    // Create sessions for the past week
                    for (int i = 0; i < 3; i++)
                    {
                        var sessionDate = today.AddDays(-(7 - i));
                        sessions.Add(new AttendanceSession
                        {
                            SectionId = section.Id,
                            InstructorId = instructor?.Id,
                            Date = sessionDate,
                            StartTime = new TimeSpan(9, 0, 0),
                            EndTime = new TimeSpan(10, 30, 0),
                            Latitude = 41.0082m, // Example coordinates (Istanbul)
                            Longitude = 28.9784m,
                            GeofenceRadius = 100m, // 100 meters
                            QrCode = $"QR-{section.Id}-{sessionDate:yyyyMMdd}",
                            Status = i < 2 ? "Completed" : "Scheduled"
                        });
                    }
                }

                if (sessions.Any())
                {
                    await context.AttendanceSessions.AddRangeAsync(sessions);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedAttendanceRecordsAsync(CampusContext context)
        {
            if (!await context.AttendanceRecords.AnyAsync())
            {
                var sessions = await context.AttendanceSessions
                    .Where(s => s.Status == "Completed")
                    .Take(3)
                    .ToListAsync();
                
                var enrollments = await context.Enrollments
                    .Where(e => e.Status == "Active")
                    .Include(e => e.Student)
                    .Take(5)
                    .ToListAsync();

                if (!sessions.Any() || !enrollments.Any())
                    return;

                var records = new List<AttendanceRecord>();

                // Create attendance records for first session
                if (sessions.Count > 0)
                {
                    var firstSession = sessions[0];
                    var sessionEnrollments = enrollments
                        .Where(e => e.SectionId == firstSession.SectionId)
                        .Take(3)
                        .ToList();

                    foreach (var enrollment in sessionEnrollments)
                    {
                        var checkInTime = firstSession.Date.Add(firstSession.StartTime).AddMinutes(new Random().Next(0, 15));
                        records.Add(new AttendanceRecord
                        {
                            SessionId = firstSession.Id,
                            StudentId = enrollment.StudentId,
                            CheckInTime = checkInTime,
                            Latitude = firstSession.Latitude,
                            Longitude = firstSession.Longitude,
                            DistanceFromCenter = new Random().Next(0, 50), // Within geofence
                            IsFlagged = false
                        });
                    }
                }

                // Create attendance records for second session with some flagged
                if (sessions.Count > 1)
                {
                    var secondSession = sessions[1];
                    var sessionEnrollments = enrollments
                        .Where(e => e.SectionId == secondSession.SectionId)
                        .Take(2)
                        .ToList();

                    foreach (var enrollment in sessionEnrollments)
                    {
                        var isLate = new Random().Next(0, 2) == 1;
                        var checkInTime = isLate 
                            ? secondSession.Date.Add(secondSession.StartTime).AddMinutes(new Random().Next(20, 45))
                            : secondSession.Date.Add(secondSession.StartTime).AddMinutes(new Random().Next(0, 10));

                        records.Add(new AttendanceRecord
                        {
                            SessionId = secondSession.Id,
                            StudentId = enrollment.StudentId,
                            CheckInTime = checkInTime,
                            Latitude = secondSession.Latitude,
                            Longitude = secondSession.Longitude,
                            DistanceFromCenter = isLate ? new Random().Next(100, 200) : new Random().Next(0, 50),
                            IsFlagged = isLate,
                            FlagReason = isLate ? "Late check-in" : null
                        });
                    }
                }

                if (records.Any())
                {
                    await context.AttendanceRecords.AddRangeAsync(records);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedExcuseRequestsAsync(CampusContext context, UserManager<User> userManager)
        {
            if (!await context.ExcuseRequests.AnyAsync())
            {
                var records = await context.AttendanceRecords
                    .Where(r => r.IsFlagged)
                    .Include(r => r.Student)
                    .Take(2)
                    .ToListAsync();

                var facultyUsers = await userManager.GetUsersInRoleAsync("Faculty");
                var reviewer = facultyUsers.FirstOrDefault();

                if (!records.Any())
                    return;

                var excuseRequests = new List<ExcuseRequest>();

                foreach (var record in records)
                {
                    excuseRequests.Add(new ExcuseRequest
                    {
                        StudentId = record.StudentId,
                        SessionId = record.SessionId,
                        Reason = "Medical appointment - doctor's note attached",
                        DocumentUrl = "/uploads/excuses/medical-note-sample.pdf",
                        Status = "Pending"
                    });
                }

                // Add one approved request
                if (records.Count > 0)
                {
                    var firstRecord = records[0];
                    var session = await context.AttendanceSessions
                        .FirstOrDefaultAsync(s => s.Id == firstRecord.SessionId);

                    if (session != null)
                    {
                        excuseRequests.Add(new ExcuseRequest
                        {
                            StudentId = firstRecord.StudentId,
                            SessionId = firstRecord.SessionId,
                            Reason = "Family emergency - approved by instructor",
                            DocumentUrl = "/uploads/excuses/emergency-note.pdf",
                            Status = "Approved",
                            ReviewedBy = reviewer?.Id,
                            ReviewedAt = DateTime.UtcNow.AddDays(-1),
                            Notes = "Approved - valid excuse"
                        });
                    }
                }

                if (excuseRequests.Any())
                {
                    await context.ExcuseRequests.AddRangeAsync(excuseRequests);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
