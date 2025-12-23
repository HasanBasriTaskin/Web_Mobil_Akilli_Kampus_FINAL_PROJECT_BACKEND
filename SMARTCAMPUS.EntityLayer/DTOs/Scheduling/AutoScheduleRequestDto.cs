namespace SMARTCAMPUS.EntityLayer.DTOs.Scheduling
{
    /// <summary>
    /// Otomatik ders programı oluşturma istek DTO'su
    /// </summary>
    public class AutoScheduleRequestDto
    {
        /// <summary>
        /// Dönem (Fall, Spring, Summer)
        /// </summary>
        public string Semester { get; set; } = null!;
        
        /// <summary>
        /// Yıl (2024, 2025, vb.)
        /// </summary>
        public int Year { get; set; }
        
        /// <summary>
        /// Programlanacak ders bölümü ID'leri (boş ise tüm dönem bölümleri)
        /// </summary>
        public List<int>? SectionIds { get; set; }
        
        /// <summary>
        /// İzin verilen zaman slotları (boş ise varsayılan slotlar kullanılır)
        /// </summary>
        public List<TimeSlotDefinitionDto>? AllowedTimeSlots { get; set; }
        
        /// <summary>
        /// Maksimum iterasyon sayısı (varsayılan: 10000)
        /// </summary>
        public int MaxIterations { get; set; } = 10000;
    }
    
    /// <summary>
    /// Zaman slotu tanımı
    /// </summary>
    public class TimeSlotDefinitionDto
    {
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
