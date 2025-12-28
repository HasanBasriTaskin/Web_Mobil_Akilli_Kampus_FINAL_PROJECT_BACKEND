namespace SMARTCAMPUS.DataAccessLayer
{
    /// <summary>
    /// Bogus seeding için varsayılan değerler.
    /// appsettings.json'da BogusSeeding section yoksa bu değerler kullanılır.
    /// </summary>
    public static class BogusDefaults
    {
        // Kullanıcı Verileri
        public const int StudentCount = 200;
        public const int FacultyCount = 60;
        
        // Akademik Veriler
        public const int CourseCount = 50;
        public const int CourseSectionCount = 100;
        public const int EnrollmentCount = 800;
        public const int AttendanceSessionCount = 400;
        public const int AttendanceRecordCount = 3200;
        public const int ScheduleCount = 200;
        
        // Yemekhane & Cüzdan
        public const int MealMenuCount = 180;
        public const int MealReservationCount = 600;
        public const int WalletCount = 200;
        public const int WalletTransactionCount = 1200;
        
        // Etkinlikler
        public const int EventCount = 80;
        public const int EventRegistrationCount = 400;
        
        // Bildirimler & IoT
        public const int NotificationCount = 800;
        public const int SensorCount = 40;
        public const int SensorReadingCount = 2000;
        
        // Genel Ayarlar
        public const int DaysOfHistory = 60;
        public const string Locale = "tr"; // Türkçe
    }
}
