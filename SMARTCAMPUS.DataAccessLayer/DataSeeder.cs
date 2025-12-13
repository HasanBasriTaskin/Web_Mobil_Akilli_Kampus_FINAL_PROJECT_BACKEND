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
    }
}
