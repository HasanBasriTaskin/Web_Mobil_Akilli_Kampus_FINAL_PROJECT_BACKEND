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
            
            // Part 3 Seed Data
            await SeedCafeteriasAsync(context);
            await SeedFoodItemsAsync(context);
            await SeedEventCategoriesAsync(context);
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
                var departments = await context.Departments.ToListAsync();
                var courses = new List<Course>();

                foreach (var dept in departments)
                {
                    // Her bölüm için 5 ders ekle
                    var deptCourses = GetCoursesForDepartment(dept.Code, dept.Id);
                    courses.AddRange(deptCourses);
                }

                if (courses.Any())
                {
                    await context.Courses.AddRangeAsync(courses);
                    await context.SaveChangesAsync();
                }

                // Bilgisayar Mühendisliği için prerequisites ekle
                var ceng201 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "CENG201");
                var ceng101 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "CENG101");
                var ceng301 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "CENG301");

                if (ceng201 != null && ceng101 != null)
                {
                    await context.CoursePrerequisites.AddAsync(new CoursePrerequisite { CourseId = ceng201.Id, PrerequisiteCourseId = ceng101.Id });
                }
                if (ceng301 != null && ceng201 != null)
                {
                    await context.CoursePrerequisites.AddAsync(new CoursePrerequisite { CourseId = ceng301.Id, PrerequisiteCourseId = ceng201.Id });
                }
                await context.SaveChangesAsync();
            }
        }

        private static List<Course> GetCoursesForDepartment(string deptCode, int deptId)
        {
            return deptCode switch
            {
                "CENG" => new List<Course>
                {
                    new Course { Code = "CENG101", Name = "Programlamaya Giriş", Description = "C# ile programlama temelleri", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "CENG201", Name = "Veri Yapıları", Description = "Diziler, bağlı listeler, ağaçlar ve graflar", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "CENG301", Name = "Algoritmalar", Description = "Algoritma tasarımı ve analizi", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "CENG401", Name = "Veritabanı Sistemleri", Description = "RDBMS, SQL ve veritabanı tasarımı", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "CENG402", Name = "Yazılım Mühendisliği", Description = "Yazılım geliştirme süreçleri ve metodolojileri", Credits = 3, ECTS = 6, DepartmentId = deptId }
                },
                "EEE" => new List<Course>
                {
                    new Course { Code = "EEE101", Name = "Elektrik Devre Temelleri", Description = "Temel elektrik devreleri ve analizi", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "EEE201", Name = "Elektronik I", Description = "Yarı iletkenler ve temel elektronik devreler", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "EEE301", Name = "Sinyal ve Sistemler", Description = "Sürekli ve ayrık zamanlı sinyaller", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "EEE401", Name = "Güç Elektroniği", Description = "Güç dönüştürücüler ve uygulamaları", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "EEE402", Name = "Kontrol Sistemleri", Description = "Otomatik kontrol teorisi ve uygulamaları", Credits = 3, ECTS = 6, DepartmentId = deptId }
                },
                "ME" => new List<Course>
                {
                    new Course { Code = "ME101", Name = "Statik", Description = "Kuvvetler, momentler ve denge", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "ME201", Name = "Dinamik", Description = "Kinematik ve kinetik", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "ME301", Name = "Termodinamik", Description = "Enerji dönüşümleri ve ısı transferi", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "ME401", Name = "Makine Elemanları", Description = "Makine parçaları tasarımı", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "ME402", Name = "Akışkanlar Mekaniği", Description = "Akışkan davranışı ve uygulamaları", Credits = 3, ECTS = 6, DepartmentId = deptId }
                },
                "CE" => new List<Course>
                {
                    new Course { Code = "CE101", Name = "İnşaat Mühendisliğine Giriş", Description = "İnşaat mühendisliği temel kavramları", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "CE201", Name = "Mukavemet", Description = "Malzeme dayanımı ve gerilme analizi", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "CE301", Name = "Yapı Statiği", Description = "Yapı sistemlerinin analizi", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "CE401", Name = "Betonarme", Description = "Betonarme yapı tasarımı", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "CE402", Name = "Zemin Mekaniği", Description = "Zemin davranışı ve temel tasarımı", Credits = 3, ECTS = 6, DepartmentId = deptId }
                },
                "IE" => new List<Course>
                {
                    new Course { Code = "IE101", Name = "Endüstri Mühendisliğine Giriş", Description = "Endüstri mühendisliği temel kavramları", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "IE201", Name = "Yöneylem Araştırması I", Description = "Lineer programlama ve optimizasyon", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "IE301", Name = "Üretim Planlama", Description = "Üretim sistemleri ve planlama", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "IE401", Name = "Kalite Kontrol", Description = "İstatistiksel kalite kontrol yöntemleri", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = "IE402", Name = "Simülasyon", Description = "Sistem simülasyonu ve modelleme", Credits = 3, ECTS = 6, DepartmentId = deptId }
                },
                _ => new List<Course>
                {
                    new Course { Code = $"{deptCode}101", Name = $"{deptCode} Giriş Dersi", Description = "Bölüme giriş dersi", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = $"{deptCode}201", Name = $"{deptCode} Temel Ders I", Description = "Temel kavramlar", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = $"{deptCode}301", Name = $"{deptCode} İleri Ders I", Description = "İleri düzey konular", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = $"{deptCode}401", Name = $"{deptCode} Uygulama Dersi", Description = "Uygulamalı çalışmalar", Credits = 3, ECTS = 6, DepartmentId = deptId },
                    new Course { Code = $"{deptCode}402", Name = $"{deptCode} Proje Dersi", Description = "Bitirme projesi", Credits = 3, ECTS = 6, DepartmentId = deptId }
                }
            };
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
                        EnrolledCount = 0
                    });
                }

                await context.CourseSections.AddRangeAsync(sections);
                await context.SaveChangesAsync();
            }
        }
        
        // Part 3 - Seed Cafeterias
        private static async Task SeedCafeteriasAsync(CampusContext context)
        {
            if (!await context.Cafeterias.AnyAsync())
            {
                var cafeterias = new List<Cafeteria>
                {
                    new Cafeteria { Name = "Ana Yemekhane", Location = "Merkez Kampüs, A Blok", Capacity = 500 },
                    new Cafeteria { Name = "Mühendislik Kantini", Location = "Mühendislik Fakültesi, B Blok", Capacity = 200 },
                    new Cafeteria { Name = "Kütüphane Kafeteryası", Location = "Merkez Kütüphane, Zemin Kat", Capacity = 100 }
                };
                await context.Cafeterias.AddRangeAsync(cafeterias);
                await context.SaveChangesAsync();
            }
        }
        
        // Part 3 - Seed FoodItems
        private static async Task SeedFoodItemsAsync(CampusContext context)
        {
            if (!await context.FoodItems.AnyAsync())
            {
                var foodItems = new List<FoodItem>
                {
                    // Çorbalar
                    new FoodItem { Name = "Mercimek Çorbası", Category = EntityLayer.Enums.MealItemCategory.Soup, Calories = 150 },
                    new FoodItem { Name = "Ezogelin Çorbası", Category = EntityLayer.Enums.MealItemCategory.Soup, Calories = 140 },
                    new FoodItem { Name = "Domates Çorbası", Category = EntityLayer.Enums.MealItemCategory.Soup, Calories = 120 },
                    new FoodItem { Name = "Tarhana Çorbası", Category = EntityLayer.Enums.MealItemCategory.Soup, Calories = 130 },
                    
                    // Ana Yemekler
                    new FoodItem { Name = "Tavuk Sote", Category = EntityLayer.Enums.MealItemCategory.MainCourse, Calories = 350 },
                    new FoodItem { Name = "Etli Kuru Fasulye", Category = EntityLayer.Enums.MealItemCategory.MainCourse, Calories = 400 },
                    new FoodItem { Name = "İzmir Köfte", Category = EntityLayer.Enums.MealItemCategory.MainCourse, Calories = 380 },
                    new FoodItem { Name = "Fırın Tavuk", Category = EntityLayer.Enums.MealItemCategory.MainCourse, Calories = 320 },
                    new FoodItem { Name = "Etli Nohut", Category = EntityLayer.Enums.MealItemCategory.MainCourse, Calories = 380 },
                    
                    // Yan Yemekler
                    new FoodItem { Name = "Pirinç Pilavı", Category = EntityLayer.Enums.MealItemCategory.SideDish, Calories = 200 },
                    new FoodItem { Name = "Bulgur Pilavı", Category = EntityLayer.Enums.MealItemCategory.SideDish, Calories = 180 },
                    new FoodItem { Name = "Makarna", Category = EntityLayer.Enums.MealItemCategory.SideDish, Calories = 220 },
                    new FoodItem { Name = "Patates Püresi", Category = EntityLayer.Enums.MealItemCategory.SideDish, Calories = 190 },
                    
                    // Salatalar
                    new FoodItem { Name = "Mevsim Salatası", Category = EntityLayer.Enums.MealItemCategory.Salad, Calories = 50 },
                    new FoodItem { Name = "Çoban Salatası", Category = EntityLayer.Enums.MealItemCategory.Salad, Calories = 60 },
                    new FoodItem { Name = "Cacık", Category = EntityLayer.Enums.MealItemCategory.Salad, Calories = 80 },
                    
                    // İçecekler
                    new FoodItem { Name = "Ayran", Category = EntityLayer.Enums.MealItemCategory.Beverage, Calories = 70 },
                    new FoodItem { Name = "Komposto", Category = EntityLayer.Enums.MealItemCategory.Beverage, Calories = 90 },
                    new FoodItem { Name = "Limonata", Category = EntityLayer.Enums.MealItemCategory.Beverage, Calories = 100 },
                    
                    // Tatlılar
                    new FoodItem { Name = "Sütlaç", Category = EntityLayer.Enums.MealItemCategory.Dessert, Calories = 200 },
                    new FoodItem { Name = "Puding", Category = EntityLayer.Enums.MealItemCategory.Dessert, Calories = 180 },
                    new FoodItem { Name = "Meyve", Category = EntityLayer.Enums.MealItemCategory.Dessert, Calories = 80 }
                };
                await context.FoodItems.AddRangeAsync(foodItems);
                await context.SaveChangesAsync();
            }
        }
        
        // Part 3 - Seed EventCategories
        private static async Task SeedEventCategoriesAsync(CampusContext context)
        {
            if (!await context.EventCategories.AnyAsync())
            {
                var categories = new List<EventCategory>
                {
                    new EventCategory { Name = "Kültürel Etkinlikler", Description = "Konserler, tiyatro, sergi ve kültürel organizasyonlar", IconName = "cultural" },
                    new EventCategory { Name = "Spor Etkinlikleri", Description = "Turnuvalar, maçlar ve spor aktiviteleri", IconName = "sports" },
                    new EventCategory { Name = "Akademik Etkinlikler", Description = "Konferanslar, seminerler ve akademik sunumlar", IconName = "academic" },
                    new EventCategory { Name = "Sosyal Etkinlikler", Description = "Öğrenci toplantıları, partiler ve sosyal organizasyonlar", IconName = "social" },
                    new EventCategory { Name = "Kariyer Etkinlikleri", Description = "Kariyer fuarları, iş görüşmeleri ve staj tanıtımları", IconName = "career" },
                    new EventCategory { Name = "Atölye Çalışmaları", Description = "Workshop, eğitim ve uygulamalı çalışmalar", IconName = "workshop" }
                };
                await context.EventCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }
        }
    }
}
