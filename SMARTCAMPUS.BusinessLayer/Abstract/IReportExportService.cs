namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    /// <summary>
    /// Rapor dışa aktarma servisi (Excel ve PDF)
    /// </summary>
    public interface IReportExportService
    {
        /// <summary>
        /// Öğrenci listesini Excel formatında dışa aktarır
        /// </summary>
        /// <param name="departmentId">Opsiyonel: Belirli bir bölüm için filtreleme</param>
        Task<byte[]> ExportStudentListToExcelAsync(int? departmentId = null);

        /// <summary>
        /// Ders not raporunu Excel formatında dışa aktarır
        /// </summary>
        Task<byte[]> ExportGradeReportToExcelAsync(int sectionId);

        /// <summary>
        /// Öğrenci transkriptini PDF formatında dışa aktarır
        /// </summary>
        Task<byte[]> ExportTranscriptToPdfAsync(int studentId);

        /// <summary>
        /// Ders devamsızlık raporunu PDF formatında dışa aktarır
        /// </summary>
        Task<byte[]> ExportAttendanceReportToPdfAsync(int sectionId);

        /// <summary>
        /// Riskli öğrenciler raporunu Excel'e aktarır
        /// </summary>
        Task<byte[]> ExportAtRiskStudentsToExcelAsync(double gpaThreshold = 2.0);
    }
}
