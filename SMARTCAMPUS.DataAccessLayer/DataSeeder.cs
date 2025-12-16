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
            
            // Part 2 Seed Data
            await SeedClassroomsAsync(context);
            await SeedCoursesAsync(context);
            await SeedCourseSectionsAsync(context);
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
                    new Classroom { Building = "Engineering Building", RoomNumber = "E101", Capacity = 50, Latitude = 41.015, Longitude = 29.045, FeaturesJson = "[\"Projector\",\"Whiteboard\"]" },
                    new Classroom { Building = "Engineering Building", RoomNumber = "E102", Capacity = 40, Latitude = 41.015, Longitude = 29.045, FeaturesJson = "[\"Projector\",\"Computer Lab\"]" },
                    new Classroom { Building = "Science Building", RoomNumber = "S201", Capacity = 60, Latitude = 41.016, Longitude = 29.044, FeaturesJson = "[\"Projector\",\"Lab Equipment\"]" },
                    new Classroom { Building = "Main Building", RoomNumber = "M301", Capacity = 100, Latitude = 41.014, Longitude = 29.046, FeaturesJson = "[\"Projector\",\"Microphone\"]" }
                };
                await context.Classrooms.AddRangeAsync(classrooms);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedCoursesAsync(CampusContext context)
        {
            if (!await context.Courses.AnyAsync())
            {
                var dept = await context.Departments.FirstOrDefaultAsync();
                var deptId = dept?.Id ?? 1;

                var courses = new List<Course>
                {
                    new Course { Code = "CS101", Name = "Introduction to Programming", Description = "Fundamentals of programming using C#", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "CS201", Name = "Data Structures", Description = "Arrays, linked lists, trees, and graphs", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "CS301", Name = "Algorithms", Description = "Algorithm design and analysis", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "CS401", Name = "Database Systems", Description = "RDBMS, SQL, and database design", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "MATH101", Name = "Calculus I", Description = "Limits, derivatives, and integrals", Credits = 4, ECTS = 7, DepartmentId = deptId }
                };
                await context.Courses.AddRangeAsync(courses);
                await context.SaveChangesAsync();

                // Add prerequisites
                var cs201 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "CS201");
                var cs101 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "CS101");
                var cs301 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "CS301");

                if (cs201 != null && cs101 != null)
                {
                    await context.CoursePrerequisites.AddAsync(new CoursePrerequisite { CourseId = cs201.Id, PrerequisiteCourseId = cs101.Id });
                }
                if (cs301 != null && cs201 != null)
                {
                    await context.CoursePrerequisites.AddAsync(new CoursePrerequisite { CourseId = cs301.Id, PrerequisiteCourseId = cs201.Id });
                }
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedCourseSectionsAsync(CampusContext context)
        {
            if (!await context.CourseSections.AnyAsync())
            {
                var faculty = await context.Faculties.FirstOrDefaultAsync();
                if (faculty == null) return;

                var courses = await context.Courses.ToListAsync();
                var sections = new List<CourseSection>();

                foreach (var course in courses)
                {
                    sections.Add(new CourseSection
                    {
                        CourseId = course.Id,
                        InstructorId = faculty.Id,
                        SectionNumber = "01",
                        Semester = "Fall",
                        Year = 2024,
                        Capacity = 40,
                        EnrolledCount = 0,
                        ScheduleJson = "[{\"day\":\"Monday\",\"startTime\":\"09:00\",\"endTime\":\"10:50\"},{\"day\":\"Wednesday\",\"startTime\":\"09:00\",\"endTime\":\"10:50\"}]"
                    });
                }

                await context.CourseSections.AddRangeAsync(sections);
                await context.SaveChangesAsync();
            }
        }
    }
}
