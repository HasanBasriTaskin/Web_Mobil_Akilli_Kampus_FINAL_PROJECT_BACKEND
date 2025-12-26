namespace SMARTCAMPUS.EntityLayer.DTOs.Scheduling
{
    /// <summary>
    /// Otomatik ders programı oluşturma sonuç DTO'su
    /// </summary>
    public class AutoScheduleResultDto
    {
        /// <summary>
        /// İşlem başarılı mı
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// İşlem mesajı
        /// </summary>
        public string Message { get; set; } = null!;
        
        /// <summary>
        /// Toplam bölüm sayısı
        /// </summary>
        public int TotalSections { get; set; }
        
        /// <summary>
        /// Başarıyla programlanan bölüm sayısı
        /// </summary>
        public int ScheduledSections { get; set; }
        
        /// <summary>
        /// Programlanamayan bölüm sayısı
        /// </summary>
        public int UnscheduledSections { get; set; }
        
        /// <summary>
        /// Oluşturulan program detayları
        /// </summary>
        public List<ScheduleDto> GeneratedSchedules { get; set; } = new();
        
        /// <summary>
        /// Programlanamayan bölümler ve nedenleri
        /// </summary>
        public List<UnscheduledSectionDto> FailedSections { get; set; } = new();
        
        /// <summary>
        /// Algoritma istatistikleri
        /// </summary>
        public AlgorithmStatisticsDto Statistics { get; set; } = new();
    }
    
    /// <summary>
    /// Programlanamayan bölüm bilgisi
    /// </summary>
    public class UnscheduledSectionDto
    {
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string SectionNumber { get; set; } = null!;
        public string Reason { get; set; } = null!;
    }
    
    /// <summary>
    /// Algoritma istatistikleri
    /// </summary>
    public class AlgorithmStatisticsDto
    {
        /// <summary>
        /// Toplam iterasyon sayısı
        /// </summary>
        public int TotalIterations { get; set; }
        
        /// <summary>
        /// Backtrack sayısı
        /// </summary>
        public int BacktrackCount { get; set; }
        
        /// <summary>
        /// Çalışma süresi (milisaniye)
        /// </summary>
        public long ElapsedMilliseconds { get; set; }
    }
}
