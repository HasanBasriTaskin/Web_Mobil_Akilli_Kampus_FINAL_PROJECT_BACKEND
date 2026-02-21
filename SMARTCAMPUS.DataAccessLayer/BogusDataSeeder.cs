using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer
{
    /// <summary>
    /// Bogus kütüphanesi ile sahte veri oluşturan seeder sınıfı.
    /// Threshold-based seeding: Mevcut veri sayısı hedefin altındaysa tamamlar.
    /// </summary>
    public class BogusDataSeeder
    {
        private readonly CampusContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BogusDataSeeder> _logger;
        
        // Faker instance'ları Türkçe locale ile
        private readonly Faker _faker;

        public BogusDataSeeder(
            CampusContext context,
            UserManager<User> userManager,
            IConfiguration configuration,
            ILogger<BogusDataSeeder> logger)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            
            // Türkçe locale ile Faker başlat
            Randomizer.Seed = new Random(8675309); // Reproducible seed
            _faker = new Faker(BogusDefaults.Locale);
        }

        /// <summary>
        /// Tüm Bogus seed işlemlerini çalıştırır.
        /// </summary>
        public async Task SeedAllAsync()
        {
            _logger.LogInformation("🌱 Bogus Data Seeding başlatılıyor...");
            
            try
            {
                // Adım 1: Kullanıcı verileri genişletme
                await SeedStudentsAsync();
                await SeedFacultyAsync();
                
                // Adım 2: Akademik veriler
                await SeedEnrollmentsAsync();
                await SeedSchedulesAsync();
                await SeedAttendanceSessionsAsync();
                await SeedAttendanceRecordsAsync();
                
                // Adım 3: Yemekhane & Cüzdan
                await SeedMealMenusAsync();
                await SeedWalletsAsync();
                await SeedWalletTransactionsAsync();
                await SeedMealReservationsAsync();
                
                // Adım 4: Etkinlikler
                await SeedEventsAsync();
                await SeedEventRegistrationsAsync();
                
                // Adım 5: Bildirimler & IoT
                await SeedNotificationsAsync();
                await SeedSensorsAsync();
                await SeedSensorReadingsAsync();
                
                _logger.LogInformation("✅ Bogus Data Seeding tamamlandı!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Bogus Data Seeding hatası!");
                throw;
            }
        }

        #region Kullanıcı Verileri

        private async Task SeedStudentsAsync()
        {
            var currentCount = await _context.Students.CountAsync();
            var targetCount = GetConfigValue("StudentCount", BogusDefaults.StudentCount);
            
            if (currentCount >= targetCount)
            {
                _logger.LogInformation("Students: {Current}/{Target} - Yeterli veri mevcut", currentCount, targetCount);
                return;
            }

            var departments = await _context.Departments.ToListAsync();
            if (!departments.Any())
            {
                _logger.LogWarning("Students: Department bulunamadı, seed atlanıyor");
                return;
            }

            var toCreate = targetCount - currentCount;
            _logger.LogInformation("Students: {ToCreate} yeni öğrenci oluşturuluyor...", toCreate);

            var studentFaker = new Faker<User>(BogusDefaults.Locale)
                .RuleFor(u => u.FullName, f => f.Name.FullName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FullName.Split(' ')[0], u.FullName.Split(' ').Last()).ToLower())
                .RuleFor(u => u.UserName, (f, u) => u.Email)
                .RuleFor(u => u.EmailConfirmed, true)
                .RuleFor(u => u.IsActive, true)
                .RuleFor(u => u.CreatedDate, f => f.Date.Past(2));

            var studentsPerDept = toCreate / departments.Count;
            var remainder = toCreate % departments.Count;

            foreach (var dept in departments)
            {
                var countForDept = studentsPerDept + (remainder-- > 0 ? 1 : 0);
                
                for (int i = 0; i < countForDept; i++)
                {
                    var user = studentFaker.Generate();
                    var result = await _userManager.CreateAsync(user, "Student123!");
                    
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Student");
                        
                        var student = new Student
                        {
                            UserId = user.Id,
                            StudentNumber = $"{DateTime.Now.Year}{_faker.Random.Number(10000, 99999)}",
                            DepartmentId = dept.Id
                        };
                        await _context.Students.AddAsync(student);
                    }
                }
            }
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Students: {Count} öğrenci oluşturuldu", toCreate);
        }

        private async Task SeedFacultyAsync()
        {
            var currentCount = await _context.Faculties.CountAsync();
            var targetCount = GetConfigValue("FacultyCount", BogusDefaults.FacultyCount);
            
            if (currentCount >= targetCount)
            {
                _logger.LogInformation("Faculty: {Current}/{Target} - Yeterli veri mevcut", currentCount, targetCount);
                return;
            }

            var departments = await _context.Departments.ToListAsync();
            if (!departments.Any())
            {
                _logger.LogWarning("Faculty: Department bulunamadı, seed atlanıyor");
                return;
            }

            var toCreate = targetCount - currentCount;
            _logger.LogInformation("Faculty: {ToCreate} yeni akademisyen oluşturuluyor...", toCreate);

            var titles = new[] { "Prof. Dr.", "Doç. Dr.", "Dr. Öğr. Üyesi", "Arş. Gör. Dr.", "Arş. Gör." };
            var buildings = new[] { "A Blok", "B Blok", "C Blok", "Mühendislik Binası", "Fen Fakültesi" };

            var facultyFaker = new Faker<User>(BogusDefaults.Locale)
                .RuleFor(u => u.FullName, f => f.Name.FullName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FullName.Split(' ')[0], u.FullName.Split(' ').Last(), "smartcampus.edu.tr").ToLower())
                .RuleFor(u => u.UserName, (f, u) => u.Email)
                .RuleFor(u => u.EmailConfirmed, true)
                .RuleFor(u => u.IsActive, true)
                .RuleFor(u => u.CreatedDate, f => f.Date.Past(5));

            var facultyPerDept = toCreate / departments.Count;
            var remainder = toCreate % departments.Count;

            foreach (var dept in departments)
            {
                var countForDept = facultyPerDept + (remainder-- > 0 ? 1 : 0);
                
                for (int i = 0; i < countForDept; i++)
                {
                    var user = facultyFaker.Generate();
                    var result = await _userManager.CreateAsync(user, "Faculty123!");
                    
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Faculty");
                        
                        var faculty = new Faculty
                        {
                            UserId = user.Id,
                            EmployeeNumber = $"F{_faker.Random.Number(1000, 9999)}",
                            Title = _faker.PickRandom(titles),
                            OfficeLocation = $"{_faker.PickRandom(buildings)} {_faker.Random.Number(100, 500)}",
                            DepartmentId = dept.Id
                        };
                        await _context.Faculties.AddAsync(faculty);
                    }
                }
            }
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Faculty: {Count} akademisyen oluşturuldu", toCreate);
        }

        #endregion

        #region Akademik Veriler

        private async Task SeedEnrollmentsAsync()
        {
            var currentCount = await _context.Enrollments.CountAsync();
            var targetCount = GetConfigValue("EnrollmentCount", BogusDefaults.EnrollmentCount);
            
            if (currentCount >= targetCount)
            {
                _logger.LogInformation("Enrollments: {Current}/{Target} - Yeterli veri mevcut", currentCount, targetCount);
                return;
            }

            var students = await _context.Students.ToListAsync();
            var sections = await _context.CourseSections.ToListAsync();
            
            if (!students.Any() || !sections.Any())
            {
                _logger.LogWarning("Enrollments: Student veya CourseSection bulunamadı, seed atlanıyor");
                return;
            }

            var toCreate = targetCount - currentCount;
            _logger.LogInformation("Enrollments: {ToCreate} yeni kayıt oluşturuluyor...", toCreate);

            var existingEnrollments = await _context.Enrollments
                .Select(e => new { e.StudentId, e.SectionId })
                .ToListAsync();
            var existingSet = existingEnrollments.Select(e => $"{e.StudentId}-{e.SectionId}").ToHashSet();

            var enrollments = new List<Enrollment>();
            var statuses = new[] { EnrollmentStatus.Enrolled, EnrollmentStatus.Completed, EnrollmentStatus.Enrolled };

            foreach (var student in students)
            {
                // Her öğrenci 3-5 derse kayıtlı
                var coursesToEnroll = _faker.Random.Number(3, 5);
                var selectedSections = _faker.PickRandom(sections, Math.Min(coursesToEnroll, sections.Count)).ToList();
                
                foreach (var section in selectedSections)
                {
                    var key = $"{student.Id}-{section.Id}";
                    if (existingSet.Contains(key)) continue;
                    
                    existingSet.Add(key);
                    
                    var status = _faker.PickRandom(statuses);
                    var enrollment = new Enrollment
                    {
                        StudentId = student.Id,
                        SectionId = section.Id,
                        Status = status,
                        EnrollmentDate = _faker.Date.Past(1)
                    };

                    // Tamamlanmış dersler için not ekle
                    if (status == EnrollmentStatus.Completed)
                    {
                        enrollment.MidtermGrade = _faker.Random.Double(40, 100);
                        enrollment.FinalGrade = _faker.Random.Double(40, 100);
                        var avg = (enrollment.MidtermGrade * 0.4 + enrollment.FinalGrade * 0.6) ?? 0;
                        enrollment.LetterGrade = GetLetterGrade(avg);
                        enrollment.GradePoint = GetGradePoint(enrollment.LetterGrade);
                    }

                    enrollments.Add(enrollment);
                    
                    if (enrollments.Count >= toCreate) break;
                }
                
                if (enrollments.Count >= toCreate) break;
            }

            await _context.Enrollments.AddRangeAsync(enrollments);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Enrollments: {Count} kayıt oluşturuldu", enrollments.Count);
        }

        private async Task SeedSchedulesAsync()
        {
            var currentCount = await _context.Schedules.CountAsync();
            var targetCount = GetConfigValue("ScheduleCount", BogusDefaults.ScheduleCount);
            
            if (currentCount >= targetCount)
            {
                _logger.LogInformation("Schedules: {Current}/{Target} - Yeterli veri mevcut", currentCount, targetCount);
                return;
            }

            var sections = await _context.CourseSections.Include(s => s.Course).ToListAsync();
            var classrooms = await _context.Classrooms.ToListAsync();
            
            if (!sections.Any() || !classrooms.Any())
            {
                _logger.LogWarning("Schedules: Section veya Classroom bulunamadı, seed atlanıyor");
                return;
            }

            var toCreate = targetCount - currentCount;
            _logger.LogInformation("Schedules: {ToCreate} yeni program oluşturuluyor...", toCreate);

            var existingSchedules = await _context.Schedules
                .Select(s => new { s.SectionId, s.DayOfWeek, s.StartTime })
                .ToListAsync();
            var existingSet = existingSchedules.Select(s => $"{s.SectionId}-{s.DayOfWeek}-{s.StartTime}").ToHashSet();

            var schedules = new List<Schedule>();
            var days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
            var startTimes = new[] { new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0), new TimeSpan(13, 0, 0), new TimeSpan(15, 0, 0) };

            foreach (var section in sections)
            {
                // Her section için 2 ders saati
                var daysForSection = _faker.PickRandom(days, 2).ToList();
                
                foreach (var day in daysForSection)
                {
                    var startTime = _faker.PickRandom(startTimes);
                    var key = $"{section.Id}-{day}-{startTime}";
                    
                    if (existingSet.Contains(key)) continue;
                    existingSet.Add(key);

                    schedules.Add(new Schedule
                    {
                        SectionId = section.Id,
                        ClassroomId = _faker.PickRandom(classrooms).Id,
                        DayOfWeek = day,
                        StartTime = startTime,
                        EndTime = startTime.Add(TimeSpan.FromMinutes(90))
                    });

                    if (schedules.Count >= toCreate) break;
                }
                
                if (schedules.Count >= toCreate) break;
            }

            await _context.Schedules.AddRangeAsync(schedules);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Schedules: {Count} program oluşturuldu", schedules.Count);
        }

        private async Task SeedAttendanceSessionsAsync()
        {
            var currentCount = await _context.AttendanceSessions.CountAsync();
            var targetCount = GetConfigValue("AttendanceSessionCount", BogusDefaults.AttendanceSessionCount);
            
            if (currentCount >= targetCount)
            {
                _logger.LogInformation("AttendanceSessions: {Current}/{Target} - Yeterli veri mevcut", currentCount, targetCount);
                return;
            }

            var sections = await _context.CourseSections.ToListAsync();
            var faculties = await _context.Faculties.ToListAsync();
            
            if (!sections.Any() || !faculties.Any())
            {
                _logger.LogWarning("AttendanceSessions: Section veya Faculty bulunamadı, seed atlanıyor");
                return;
            }

            var toCreate = targetCount - currentCount;
            _logger.LogInformation("AttendanceSessions: {ToCreate} yeni oturum oluşturuluyor...", toCreate);

            var daysOfHistory = GetConfigValue("DaysOfHistory", BogusDefaults.DaysOfHistory);
            var sessions = new List<AttendanceSession>();

            foreach (var section in sections)
            {
                // Her section için son N gün içinde rastgele oturumlar
                var sessionsPerSection = toCreate / sections.Count;
                
                for (int i = 0; i < sessionsPerSection && sessions.Count < toCreate; i++)
                {
                    var date = _faker.Date.Recent(daysOfHistory);
                    var startTime = new TimeSpan(_faker.Random.Number(9, 16), 0, 0);
                    
                    sessions.Add(new AttendanceSession
                    {
                        SectionId = section.Id,
                        InstructorId = section.InstructorId,
                        Date = date,
                        StartTime = startTime,
                        EndTime = startTime.Add(TimeSpan.FromMinutes(90)),
                        Latitude = 41.015 + _faker.Random.Double(-0.01, 0.01),
                        Longitude = 29.045 + _faker.Random.Double(-0.01, 0.01),
                        GeofenceRadius = _faker.Random.Number(10, 30),
                        QRCode = _faker.Random.AlphaNumeric(32),
                        Status = AttendanceSessionStatus.Closed
                    });
                }
            }

            await _context.AttendanceSessions.AddRangeAsync(sessions);
            await _context.SaveChangesAsync();
            _logger.LogInformation("AttendanceSessions: {Count} oturum oluşturuldu", sessions.Count);
        }

        private async Task SeedAttendanceRecordsAsync()
        {
            var currentCount = await _context.AttendanceRecords.CountAsync();
            var targetCount = GetConfigValue("AttendanceRecordCount", BogusDefaults.AttendanceRecordCount);
            
            if (currentCount >= targetCount)
            {
                _logger.LogInformation("AttendanceRecords: {Current}/{Target} - Yeterli veri mevcut", currentCount, targetCount);
                return;
            }

            var sessions = await _context.AttendanceSessions.Include(s => s.Section).ToListAsync();
            
            if (!sessions.Any())
            {
                _logger.LogWarning("AttendanceRecords: AttendanceSession bulunamadı, seed atlanıyor");
                return;
            }

            var toCreate = targetCount - currentCount;
            _logger.LogInformation("AttendanceRecords: {ToCreate} yeni kayıt oluşturuluyor...", toCreate);

            // Her session için kayıtlı öğrencileri bul
            var enrollments = await _context.Enrollments
                .Where(e => e.Status == EnrollmentStatus.Enrolled || e.Status == EnrollmentStatus.Completed)
                .ToListAsync();
            
            var enrollmentsBySection = enrollments.GroupBy(e => e.SectionId).ToDictionary(g => g.Key, g => g.ToList());
            
            var existingRecords = await _context.AttendanceRecords
                .Select(r => new { r.SessionId, r.StudentId })
                .ToListAsync();
            var existingSet = existingRecords.Select(r => $"{r.SessionId}-{r.StudentId}").ToHashSet();

            var records = new List<AttendanceRecord>();

            foreach (var session in sessions)
            {
                if (!enrollmentsBySection.TryGetValue(session.SectionId, out var sectionEnrollments))
                    continue;

                foreach (var enrollment in sectionEnrollments)
                {
                    var key = $"{session.Id}-{enrollment.StudentId}";
                    if (existingSet.Contains(key)) continue;
                    
                    existingSet.Add(key);
                    
                    // %80 katılım oranı - sadece katılan öğrenciler için kayıt oluştur
                    if (!_faker.Random.Bool(0.8f)) continue;
                    
                    records.Add(new AttendanceRecord
                    {
                        SessionId = session.Id,
                        StudentId = enrollment.StudentId,
                        CheckInTime = session.Date.Add(session.StartTime).AddMinutes(_faker.Random.Number(0, 15)),
                        Latitude = session.Latitude + _faker.Random.Double(-0.001, 0.001),
                        Longitude = session.Longitude + _faker.Random.Double(-0.001, 0.001),
                        DistanceFromCenter = _faker.Random.Double(0, 15),
                        IsFlagged = _faker.Random.Bool(0.05f),
                        IsMockLocation = false,
                        FraudScore = 0
                    });

                    if (records.Count >= toCreate) break;
                }
                
                if (records.Count >= toCreate) break;
            }

            await _context.AttendanceRecords.AddRangeAsync(records);
            await _context.SaveChangesAsync();
            _logger.LogInformation("AttendanceRecords: {Count} kayıt oluşturuldu", records.Count);
        }

        #endregion

        #region Yemekhane & Cüzdan

        private async Task SeedMealMenusAsync()
        {
            var currentCount = await _context.MealMenus.CountAsync();
            var targetCount = GetConfigValue("MealMenuCount", BogusDefaults.MealMenuCount);
            
            if (currentCount >= targetCount)
            {
                _logger.LogInformation("MealMenus: {Current}/{Target} - Yeterli veri mevcut", currentCount, targetCount);
                return;
            }

            var cafeterias = await _context.Cafeterias.ToListAsync();
            var foodItems = await _context.FoodItems.ToListAsync();
            
            if (!cafeterias.Any() || !foodItems.Any())
            {
                _logger.LogWarning("MealMenus: Cafeteria veya FoodItem bulunamadı, seed atlanıyor");
                return;
            }

            var toCreate = targetCount - currentCount;
            _logger.LogInformation("MealMenus: {ToCreate} yeni menü oluşturuluyor...", toCreate);

            var daysOfHistory = GetConfigValue("DaysOfHistory", BogusDefaults.DaysOfHistory);
            var mealTypes = new[] { MealType.Breakfast, MealType.Lunch, MealType.Dinner };
            
            var existingMenus = await _context.MealMenus
                .Select(m => new { m.CafeteriaId, m.Date, m.MealType })
                .ToListAsync();
            var existingSet = existingMenus.Select(m => $"{m.CafeteriaId}-{m.Date:yyyyMMdd}-{m.MealType}").ToHashSet();

            var menus = new List<MealMenu>();
            var menuItems = new List<MealMenuItem>();

            for (int day = 0; day < daysOfHistory && menus.Count < toCreate; day++)
            {
                var date = DateTime.Today.AddDays(-day);
                
                foreach (var cafeteria in cafeterias)
                {
                    foreach (var mealType in mealTypes)
                    {
                        var key = $"{cafeteria.Id}-{date:yyyyMMdd}-{mealType}";
                        if (existingSet.Contains(key)) continue;
                        
                        existingSet.Add(key);

                        var menu = new MealMenu
                        {
                            CafeteriaId = cafeteria.Id,
                            Date = date,
                            MealType = mealType,
                            Price = _faker.Random.Decimal(25, 50),
                            IsPublished = true
                        };
                        menus.Add(menu);

                        if (menus.Count >= toCreate) break;
                    }
                    if (menus.Count >= toCreate) break;
                }
            }

            await _context.MealMenus.AddRangeAsync(menus);
            await _context.SaveChangesAsync();

            // Her menü için 4-6 yemek ekle
            foreach (var menu in menus)
            {
                var selectedItems = _faker.PickRandom(foodItems, _faker.Random.Number(4, 6)).ToList();
                foreach (var item in selectedItems)
                {
                    menuItems.Add(new MealMenuItem
                    {
                        MenuId = menu.Id,
                        FoodItemId = item.Id,
                        OrderIndex = selectedItems.IndexOf(item)
                    });
                }
            }

            await _context.MealMenuItems.AddRangeAsync(menuItems);
            await _context.SaveChangesAsync();
            _logger.LogInformation("MealMenus: {MenuCount} menü, {ItemCount} menü öğesi oluşturuldu", menus.Count, menuItems.Count);
        }

        private async Task SeedWalletsAsync()
        {
            var currentCount = await _context.Wallets.CountAsync();
            var targetCount = GetConfigValue("WalletCount", BogusDefaults.WalletCount);
            
            if (currentCount >= targetCount)
            {
                _logger.LogInformation("Wallets: {Current}/{Target} - Yeterli veri mevcut", currentCount, targetCount);
                return;
            }

            var students = await _context.Students.Include(s => s.User).ToListAsync();
            var existingWalletUserIds = await _context.Wallets.Select(w => w.UserId).ToListAsync();
            var existingSet = existingWalletUserIds.ToHashSet();

            var toCreate = Math.Min(targetCount - currentCount, students.Count(s => !existingSet.Contains(s.UserId)));
            _logger.LogInformation("Wallets: {ToCreate} yeni cüzdan oluşturuluyor...", toCreate);

            var wallets = new List<Wallet>();

            foreach (var student in students)
            {
                if (existingSet.Contains(student.UserId)) continue;
                
                wallets.Add(new Wallet
                {
                    UserId = student.UserId,
                    Balance = _faker.Random.Decimal(0, 500),
                    Currency = "TRY"
                });

                if (wallets.Count >= toCreate) break;
            }

            await _context.Wallets.AddRangeAsync(wallets);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Wallets: {Count} cüzdan oluşturuldu", wallets.Count);
        }

        private async Task SeedWalletTransactionsAsync()
        {
            var currentCount = await _context.WalletTransactions.CountAsync();
            var targetCount = GetConfigValue("WalletTransactionCount", BogusDefaults.WalletTransactionCount);
            
            if (currentCount >= targetCount)
            {
                _logger.LogInformation("WalletTransactions: {Current}/{Target} - Yeterli veri mevcut", currentCount, targetCount);
                return;
            }

            var wallets = await _context.Wallets.ToListAsync();
            
            if (!wallets.Any())
            {
                _logger.LogWarning("WalletTransactions: Wallet bulunamadı, seed atlanıyor");
                return;
            }

            var toCreate = targetCount - currentCount;
            _logger.LogInformation("WalletTransactions: {ToCreate} yeni işlem oluşturuluyor...", toCreate);

            var daysOfHistory = GetConfigValue("DaysOfHistory", BogusDefaults.DaysOfHistory);
            var transactions = new List<WalletTransaction>();
            var descriptions = new[]
            {
                "Bakiye yükleme", "Yemek ödemesi", "Etkinlik ödemesi", 
                "Fotokopi harcaması", "Kantin harcaması", "Online yükleme"
            };

            foreach (var wallet in wallets)
            {
                var transPerWallet = toCreate / wallets.Count;
                var balance = wallet.Balance;
                
                for (int i = 0; i < transPerWallet && transactions.Count < toCreate; i++)
                {
                    var isCredit = _faker.Random.Bool(0.3f); // %30 yükleme, %70 harcama
                    var amount = _faker.Random.Decimal(10, 100);
                    
                    if (!isCredit && balance < amount)
                    {
                        isCredit = true; // Bakiye yetersizse yükleme yap
                    }

                    if (isCredit)
                        balance += amount;
                    else
                        balance -= amount;

                    transactions.Add(new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Type = isCredit ? TransactionType.Credit : TransactionType.Debit,
                        Amount = amount,
                        BalanceAfter = balance,
                        ReferenceType = isCredit ? ReferenceType.TopUp : ReferenceType.MealReservation,
                        Description = _faker.PickRandom(descriptions),
                        TransactionDate = _faker.Date.Recent(daysOfHistory)
                    });
                }
            }

            await _context.WalletTransactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
            _logger.LogInformation("WalletTransactions: {Count} işlem oluşturuldu", transactions.Count);
        }

        private async Task SeedMealReservationsAsync()
        {
            var currentCount = await _context.MealReservations.CountAsync();
            var targetCount = GetConfigValue("MealReservationCount", BogusDefaults.MealReservationCount);
            
            if (currentCount >= targetCount)
            {
                _logger.LogInformation("MealReservations: {Current}/{Target} - Yeterli veri mevcut", currentCount, targetCount);
                return;
            }

            var students = await _context.Students.ToListAsync();
            var menus = await _context.MealMenus.Where(m => m.IsPublished).ToListAsync();
            
            if (!students.Any() || !menus.Any())
            {
                _logger.LogWarning("MealReservations: Student veya MealMenu bulunamadı, seed atlanıyor");
                return;
            }

            var toCreate = targetCount - currentCount;
            _logger.LogInformation("MealReservations: {ToCreate} yeni rezervasyon oluşturuluyor...", toCreate);

            var existingReservations = await _context.MealReservations
                .Select(r => new { r.UserId, r.Date, r.MealType })
                .ToListAsync();
            var existingSet = existingReservations.Select(r => $"{r.UserId}-{r.Date:yyyyMMdd}-{r.MealType}").ToHashSet();

            var reservations = new List<MealReservation>();
            var statuses = new[] { MealReservationStatus.Reserved, MealReservationStatus.Used, MealReservationStatus.Reserved };
            var localExistingSet = new HashSet<string>();

            foreach (var student in students)
            {
                var menusToReserve = _faker.PickRandom(menus, _faker.Random.Number(1, 5)).ToList();
                
                foreach (var menu in menusToReserve)
                {
                    // Unique key: UserId + Date + MealType
                    var key = $"{student.UserId}-{menu.Date:yyyyMMdd}-{menu.MealType}";
                    if (existingSet.Contains(key) || localExistingSet.Contains(key)) continue;
                    
                    existingSet.Add(key);
                    localExistingSet.Add(key);

                    var status = _faker.PickRandom(statuses);
                    reservations.Add(new MealReservation
                    {
                        UserId = student.UserId,
                        MenuId = menu.Id,
                        CafeteriaId = menu.CafeteriaId,
                        MealType = menu.MealType,
                        Date = menu.Date,
                        Status = status,
                        QRCode = Guid.NewGuid().ToString("N").Substring(0, 16),
                        UsedAt = status == MealReservationStatus.Used ? menu.Date.AddHours(12) : null
                    });

                    if (reservations.Count >= toCreate) break;
                }
                
                if (reservations.Count >= toCreate) break;
            }

            await _context.MealReservations.AddRangeAsync(reservations);
            await _context.SaveChangesAsync();
            _logger.LogInformation("MealReservations: {Count} rezervasyon oluşturuldu", reservations.Count);
        }

        #endregion

        #region Etkinlikler

        private async Task SeedEventsAsync()
        {
            var currentCount = await _context.Events.CountAsync();
            var targetCount = GetConfigValue("EventCount", BogusDefaults.EventCount);
            
            if (currentCount >= targetCount)
            {
                _logger.LogInformation("Events: {Current}/{Target} - Yeterli veri mevcut", currentCount, targetCount);
                return;
            }

            var categories = await _context.EventCategories.ToListAsync();
            var users = await _context.Users.ToListAsync();
            
            if (!categories.Any() || !users.Any())
            {
                _logger.LogWarning("Events: EventCategory veya User bulunamadı, seed atlanıyor");
                return;
            }

            var toCreate = targetCount - currentCount;
            _logger.LogInformation("Events: {ToCreate} yeni etkinlik oluşturuluyor...", toCreate);

            var locations = new[]
            {
                "Konferans Salonu A", "Konferans Salonu B", "Amfi Tiyatro",
                "Spor Salonu", "Açık Hava Sahnesi", "Kütüphane Toplantı Odası",
                "Öğrenci Merkezi", "Mühendislik Fakültesi Fuaye"
            };

            var eventTitles = new[]
            {
                "Kariyer Günleri", "Yazılım Geliştirme Workshop", "Girişimcilik Semineri",
                "Bahar Şenliği", "Mezuniyet Resepsiyonu", "Bilim Fuarı",
                "Kültür Festivali", "Spor Turnuvası", "Müzik Konseri",
                "Teknoloji Zirvesi", "Sanat Sergisi", "Tiyatro Gösterisi"
            };

            var events = new List<Event>();

            for (int i = 0; i < toCreate; i++)
            {
                var isPast = _faker.Random.Bool(0.5f);
                var startDate = isPast 
                    ? _faker.Date.Recent(60) 
                    : _faker.Date.Soon(60);
                var duration = _faker.Random.Number(2, 8);
                var capacity = _faker.Random.Number(50, 500);

                events.Add(new Event
                {
                    Title = $"{_faker.PickRandom(eventTitles)} {_faker.Random.Number(1, 10)}",
                    Description = _faker.Lorem.Paragraphs(2),
                    CategoryId = _faker.PickRandom(categories).Id,
                    StartDate = startDate,
                    EndDate = startDate.AddHours(duration),
                    Location = _faker.PickRandom(locations),
                    Capacity = capacity,
                    RegisteredCount = isPast ? _faker.Random.Number(10, capacity) : _faker.Random.Number(0, capacity / 2),
                    Price = _faker.Random.Bool(0.7f) ? 0 : _faker.Random.Decimal(10, 100),
                    CreatedByUserId = _faker.PickRandom(users).Id
                });
            }

            await _context.Events.AddRangeAsync(events);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Events: {Count} etkinlik oluşturuldu", events.Count);
        }

        private async Task SeedEventRegistrationsAsync()
        {
            var currentCount = await _context.EventRegistrations.CountAsync();
            var targetCount = GetConfigValue("EventRegistrationCount", BogusDefaults.EventRegistrationCount);
            
            if (currentCount >= targetCount)
            {
                _logger.LogInformation("EventRegistrations: {Current}/{Target} - Yeterli veri mevcut", currentCount, targetCount);
                return;
            }

            var students = await _context.Students.ToListAsync();
            var events = await _context.Events.ToListAsync();
            
            if (!students.Any() || !events.Any())
            {
                _logger.LogWarning("EventRegistrations: Student veya Event bulunamadı, seed atlanıyor");
                return;
            }

            var toCreate = targetCount - currentCount;
            _logger.LogInformation("EventRegistrations: {ToCreate} yeni kayıt oluşturuluyor...", toCreate);

            var existingRegs = await _context.EventRegistrations
                .Select(r => new { r.UserId, r.EventId })
                .ToListAsync();
            var existingSet = existingRegs.Select(r => $"{r.UserId}-{r.EventId}").ToHashSet();

            var registrations = new List<EventRegistration>();

            foreach (var evt in events)
            {
                var regsPerEvent = toCreate / events.Count;
                var selectedStudents = _faker.PickRandom(students, Math.Min(regsPerEvent, students.Count)).ToList();
                
                foreach (var student in selectedStudents)
                {
                    var key = $"{student.UserId}-{evt.Id}";
                    if (existingSet.Contains(key)) continue;
                    
                    existingSet.Add(key);

                    var isCheckedIn = evt.EndDate < DateTime.Now && _faker.Random.Bool(0.8f);
                    registrations.Add(new EventRegistration
                    {
                        UserId = student.UserId,
                        EventId = evt.Id,
                        RegistrationDate = _faker.Date.Between(evt.CreatedDate, evt.StartDate),
                        QRCode = Guid.NewGuid().ToString("N").Substring(0, 16),
                        CheckedIn = isCheckedIn,
                        CheckedInAt = isCheckedIn ? evt.StartDate.AddMinutes(_faker.Random.Number(0, 30)) : null
                    });

                    if (registrations.Count >= toCreate) break;
                }
                
                if (registrations.Count >= toCreate) break;
            }

            await _context.EventRegistrations.AddRangeAsync(registrations);
            await _context.SaveChangesAsync();
            _logger.LogInformation("EventRegistrations: {Count} kayıt oluşturuldu", registrations.Count);
        }

        #endregion

        #region Bildirimler & IoT

        private async Task SeedNotificationsAsync()
        {
            var currentCount = await _context.Notifications.CountAsync();
            var targetCount = GetConfigValue("NotificationCount", BogusDefaults.NotificationCount);
            
            if (currentCount >= targetCount)
            {
                _logger.LogInformation("Notifications: {Current}/{Target} - Yeterli veri mevcut", currentCount, targetCount);
                return;
            }

            var users = await _context.Users.ToListAsync();
            
            if (!users.Any())
            {
                _logger.LogWarning("Notifications: User bulunamadı, seed atlanıyor");
                return;
            }

            var toCreate = targetCount - currentCount;
            _logger.LogInformation("Notifications: {ToCreate} yeni bildirim oluşturuluyor...", toCreate);

            var notificationTemplates = new[]
            {
                ("Ders Hatırlatması", "Yarın {0} dersiniz var.", NotificationCategory.Academic),
                ("Yoklama Uyarısı", "{0} dersine katılımınız kaydedildi.", NotificationCategory.Attendance),
                ("Yemek Menüsü", "Bugünkü menü yayınlandı: {0}", NotificationCategory.Meal),
                ("Etkinlik Daveti", "{0} etkinliğine davetlisiniz!", NotificationCategory.Event),
                ("Ödeme Bildirimi", "Cüzdanınıza {0} TL yüklendi.", NotificationCategory.Payment),
                ("Sistem Bildirimi", "{0}", NotificationCategory.System)
            };

            var notifications = new List<Notification>();
            var daysOfHistory = GetConfigValue("DaysOfHistory", BogusDefaults.DaysOfHistory);

            foreach (var user in users)
            {
                var notifsPerUser = toCreate / users.Count;
                
                for (int i = 0; i < notifsPerUser && notifications.Count < toCreate; i++)
                {
                    var template = _faker.PickRandom(notificationTemplates);
                    var isRead = _faker.Random.Bool(0.6f);
                    var createdDate = _faker.Date.Recent(daysOfHistory);

                    notifications.Add(new Notification
                    {
                        UserId = user.Id,
                        Title = template.Item1,
                        Message = string.Format(template.Item2, _faker.Lorem.Word()),
                        Type = _faker.PickRandom<NotificationType>(),
                        Category = template.Item3,
                        IsRead = isRead,
                        ReadAt = isRead ? createdDate.AddHours(_faker.Random.Number(1, 24)) : null,
                        CreatedDate = createdDate
                    });
                }
            }

            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Notifications: {Count} bildirim oluşturuldu", notifications.Count);
        }

        private async Task SeedSensorsAsync()
        {
            var currentCount = await _context.Sensors.CountAsync();
            var targetCount = GetConfigValue("SensorCount", BogusDefaults.SensorCount);
            
            if (currentCount >= targetCount)
            {
                _logger.LogInformation("Sensors: {Current}/{Target} - Yeterli veri mevcut", currentCount, targetCount);
                return;
            }

            var classrooms = await _context.Classrooms.ToListAsync();
            var toCreate = targetCount - currentCount;
            _logger.LogInformation("Sensors: {ToCreate} yeni sensör oluşturuluyor...", toCreate);

            var sensorTypes = Enum.GetValues<SensorType>();
            var sensors = new List<Sensor>();

            for (int i = 0; i < toCreate; i++)
            {
                var sensorType = _faker.PickRandom(sensorTypes);
                var classroom = classrooms.Any() && _faker.Random.Bool(0.7f) ? _faker.PickRandom(classrooms) : null;

                sensors.Add(new Sensor
                {
                    SensorId = $"SNS-{sensorType.ToString().Substring(0, 3).ToUpper()}-{_faker.Random.Number(1000, 9999)}",
                    Name = $"{sensorType} Sensör {i + 1}",
                    Type = sensorType,
                    Location = classroom?.Building ?? _faker.PickRandom(new[] { "Koridor", "Giriş", "Otopark", "Bahçe" }),
                    ClassroomId = classroom?.Id,
                    IsOnline = _faker.Random.Bool(0.9f),
                    LastReading = _faker.Date.Recent(1)
                });
            }

            await _context.Sensors.AddRangeAsync(sensors);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Sensors: {Count} sensör oluşturuldu", sensors.Count);
        }

        private async Task SeedSensorReadingsAsync()
        {
            var currentCount = await _context.SensorReadings.CountAsync();
            var targetCount = GetConfigValue("SensorReadingCount", BogusDefaults.SensorReadingCount);
            
            if (currentCount >= targetCount)
            {
                _logger.LogInformation("SensorReadings: {Current}/{Target} - Yeterli veri mevcut", currentCount, targetCount);
                return;
            }

            var sensors = await _context.Sensors.ToListAsync();
            
            if (!sensors.Any())
            {
                _logger.LogWarning("SensorReadings: Sensor bulunamadı, seed atlanıyor");
                return;
            }

            var toCreate = targetCount - currentCount;
            _logger.LogInformation("SensorReadings: {ToCreate} yeni okuma oluşturuluyor...", toCreate);

            var readings = new List<SensorReading>();

            foreach (var sensor in sensors)
            {
                var readingsPerSensor = toCreate / sensors.Count;
                
                for (int i = 0; i < readingsPerSensor && readings.Count < toCreate; i++)
                {
                    var (value, unit) = GetSensorValue(sensor.Type);
                    
                    readings.Add(new SensorReading
                    {
                        SensorId = sensor.Id,
                        Value = value,
                        Unit = unit,
                        Timestamp = _faker.Date.Recent(7)
                    });
                }
            }

            await _context.SensorReadings.AddRangeAsync(readings);
            await _context.SaveChangesAsync();
            _logger.LogInformation("SensorReadings: {Count} okuma oluşturuldu", readings.Count);
        }

        #endregion

        #region Helper Methods

        private int GetConfigValue(string key, int defaultValue)
        {
            var section = _configuration.GetSection("BogusSeeding");
            if (!section.Exists()) return defaultValue;
            
            var value = section[key];
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        private static string GetLetterGrade(double average)
        {
            return average switch
            {
                >= 90 => "AA",
                >= 85 => "BA",
                >= 80 => "BB",
                >= 75 => "CB",
                >= 70 => "CC",
                >= 65 => "DC",
                >= 60 => "DD",
                >= 55 => "FD",
                _ => "FF"
            };
        }

        private static double GetGradePoint(string letterGrade)
        {
            return letterGrade switch
            {
                "AA" => 4.0,
                "BA" => 3.5,
                "BB" => 3.0,
                "CB" => 2.5,
                "CC" => 2.0,
                "DC" => 1.5,
                "DD" => 1.0,
                "FD" => 0.5,
                _ => 0.0
            };
        }

        private (double value, string unit) GetSensorValue(SensorType type)
        {
            return type switch
            {
                SensorType.Temperature => (_faker.Random.Double(18, 28), "°C"),
                SensorType.Humidity => (_faker.Random.Double(30, 70), "%"),
                SensorType.Occupancy => (_faker.Random.Double(0, 100), "kişi"),
                SensorType.Energy => (_faker.Random.Double(100, 5000), "kWh"),
                SensorType.AirQuality => (_faker.Random.Double(0, 500), "AQI"),
                SensorType.Light => (_faker.Random.Double(100, 1000), "lux"),
                _ => (_faker.Random.Double(0, 100), "birim")
            };
        }

        #endregion
    }
}
