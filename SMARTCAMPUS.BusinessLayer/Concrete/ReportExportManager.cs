using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    /// <summary>
    /// Excel ve PDF rapor dışa aktarma servisi
    /// </summary>
    public class ReportExportManager : IReportExportService
    {
        private readonly CampusContext _context;

        public ReportExportManager(CampusContext context)
        {
            _context = context;
            // QuestPDF Community License
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> ExportStudentListToExcelAsync(int? departmentId = null)
        {
            var query = _context.Students
                .Include(s => s.User)
                .Include(s => s.Department)
                .Where(s => s.IsActive);

            if (departmentId.HasValue)
            {
                query = query.Where(s => s.DepartmentId == departmentId.Value);
            }

            var students = await query.OrderBy(s => s.StudentNumber).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Öğrenci Listesi");

            // Header
            worksheet.Cell(1, 1).Value = "Öğrenci No";
            worksheet.Cell(1, 2).Value = "Ad Soyad";
            worksheet.Cell(1, 3).Value = "E-posta";
            worksheet.Cell(1, 4).Value = "Bölüm";
            worksheet.Cell(1, 5).Value = "GPA";
            worksheet.Cell(1, 6).Value = "CGPA";

            // Header styling
            var headerRange = worksheet.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

            // Data
            for (int i = 0; i < students.Count; i++)
            {
                var student = students[i];
                worksheet.Cell(i + 2, 1).Value = student.StudentNumber;
                worksheet.Cell(i + 2, 2).Value = student.User?.FullName ?? "";
                worksheet.Cell(i + 2, 3).Value = student.User?.Email ?? "";
                worksheet.Cell(i + 2, 4).Value = student.Department?.Name ?? "";
                worksheet.Cell(i + 2, 5).Value = student.GPA;
                worksheet.Cell(i + 2, 6).Value = student.CGPA;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportGradeReportToExcelAsync(int sectionId)
        {
            var section = await _context.CourseSections
                .Include(s => s.Course)
                .Include(s => s.Instructor)
                    .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null)
            {
                throw new ArgumentException("Ders bulunamadı");
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Where(e => e.SectionId == sectionId && e.IsActive)
                .OrderBy(e => e.Student.StudentNumber)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Not Raporu");

            // Course info
            worksheet.Cell(1, 1).Value = $"Ders: {section.Course?.Code} - {section.Course?.Name}";
            worksheet.Cell(2, 1).Value = $"Öğretim Üyesi: {section.Instructor?.User?.FullName ?? "N/A"}";
            worksheet.Cell(3, 1).Value = $"Dönem: {section.Semester} {section.Year}";

            // Header (row 5)
            worksheet.Cell(5, 1).Value = "Öğrenci No";
            worksheet.Cell(5, 2).Value = "Ad Soyad";
            worksheet.Cell(5, 3).Value = "Vize";
            worksheet.Cell(5, 4).Value = "Final";
            worksheet.Cell(5, 5).Value = "Ortalama";
            worksheet.Cell(5, 6).Value = "Harf Notu";

            var headerRange = worksheet.Range(5, 1, 5, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;

            // Data
            for (int i = 0; i < enrollments.Count; i++)
            {
                var enrollment = enrollments[i];
                worksheet.Cell(i + 6, 1).Value = enrollment.Student?.StudentNumber ?? "";
                worksheet.Cell(i + 6, 2).Value = enrollment.Student?.User?.FullName ?? "";
                worksheet.Cell(i + 6, 3).Value = enrollment.MidtermGrade?.ToString("F1") ?? "-";
                worksheet.Cell(i + 6, 4).Value = enrollment.FinalGrade?.ToString("F1") ?? "-";
                
                // Ortalamayı hesapla (Vize %40, Final %60)
                var average = enrollment.MidtermGrade.HasValue && enrollment.FinalGrade.HasValue
                    ? (enrollment.MidtermGrade.Value * 0.4 + enrollment.FinalGrade.Value * 0.6)
                    : (double?)null;
                worksheet.Cell(i + 6, 5).Value = average?.ToString("F2") ?? "-";
                worksheet.Cell(i + 6, 6).Value = enrollment.LetterGrade ?? "-";
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportTranscriptToPdfAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Id == studentId && s.IsActive);

            if (student == null)
            {
                throw new ArgumentException("Öğrenci bulunamadı");
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.Section)
                    .ThenInclude(s => s.Course)
                .Where(e => e.StudentId == studentId && e.IsActive && e.LetterGrade != null)
                .OrderBy(e => e.Section.Year)
                .ThenBy(e => e.Section.Semester)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().Text("SMART CAMPUS ÜNİVERSİTESİ").Bold().FontSize(16).AlignCenter();
                        column.Item().Text("ÖĞRENCİ TRANSKRİPTİ").FontSize(14).AlignCenter();
                        column.Item().PaddingVertical(10);
                    });

                    page.Content().Column(column =>
                    {
                        // Student Info
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"Öğrenci No: {student.StudentNumber}").Bold();
                                col.Item().Text($"Ad Soyad: {student.User?.FullName ?? "N/A"}");
                            });
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"Bölüm: {student.Department?.Name ?? "N/A"}");
                                col.Item().Text($"Genel Not Ort.: {student.CGPA:F2}").Bold();
                            });
                        });

                        column.Item().PaddingVertical(10);

                        // Grades Table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Ders Kodu
                                columns.RelativeColumn(4); // Ders Adı
                                columns.RelativeColumn(1); // Kredi
                                columns.RelativeColumn(1); // Harf Notu
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Ders Kodu").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Ders Adı").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Kredi").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Not").Bold();
                            });

                            // Data
                            foreach (var enrollment in enrollments)
                            {
                                table.Cell().BorderBottom(1).Padding(3).Text(enrollment.Section?.Course?.Code ?? "");
                                table.Cell().BorderBottom(1).Padding(3).Text(enrollment.Section?.Course?.Name ?? "");
                                table.Cell().BorderBottom(1).Padding(3).Text(enrollment.Section?.Course?.Credits.ToString() ?? "");
                                table.Cell().BorderBottom(1).Padding(3).Text(enrollment.LetterGrade ?? "");
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span($"Oluşturulma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}");
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> ExportAttendanceReportToPdfAsync(int sectionId)
        {
            var section = await _context.CourseSections
                .Include(s => s.Course)
                .Include(s => s.Instructor)
                    .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null)
            {
                throw new ArgumentException("Ders bulunamadı");
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Where(e => e.SectionId == sectionId && e.IsActive && e.Status == EnrollmentStatus.Enrolled)
                .ToListAsync();

            var sessions = await _context.AttendanceSessions
                .Where(s => s.SectionId == sectionId && s.IsActive)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var records = await _context.AttendanceRecords
                .Where(r => r.Session.SectionId == sectionId && r.IsActive)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().Text("DEVAMSIZLIK RAPORU").Bold().FontSize(14).AlignCenter();
                        column.Item().Text($"{section.Course?.Code} - {section.Course?.Name}").AlignCenter();
                        column.Item().Text($"Dönem: {section.Semester} {section.Year}").AlignCenter();
                    });

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Item().Text($"Toplam Oturum: {sessions.Count}").Bold();
                        column.Item().PaddingVertical(5);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Öğrenci No
                                columns.RelativeColumn(3); // Ad Soyad
                                columns.RelativeColumn(1); // Katılım
                                columns.RelativeColumn(1); // Devamsız
                                columns.RelativeColumn(1); // Oran
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Öğrenci No").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Ad Soyad").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Katılım").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Devamsız").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Oran (%)").Bold();
                            });

                            foreach (var enrollment in enrollments.OrderBy(e => e.Student?.StudentNumber))
                            {
                                var studentRecords = records.Where(r => r.StudentId == enrollment.StudentId).ToList();
                                var attendedCount = studentRecords.Count(r => !r.IsFlagged);
                                var absentCount = sessions.Count - studentRecords.Count + studentRecords.Count(r => r.IsFlagged);
                                var rate = sessions.Count > 0 ? (double)attendedCount / sessions.Count * 100 : 0;

                                table.Cell().BorderBottom(1).Padding(2).Text(enrollment.Student?.StudentNumber ?? "");
                                table.Cell().BorderBottom(1).Padding(2).Text(enrollment.Student?.User?.FullName ?? "");
                                table.Cell().BorderBottom(1).Padding(2).Text(attendedCount.ToString());
                                table.Cell().BorderBottom(1).Padding(2).Text(absentCount.ToString());
                                table.Cell().BorderBottom(1).Padding(2).Text($"{rate:F1}");
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text($"Oluşturulma: {DateTime.Now:dd.MM.yyyy HH:mm}");
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> ExportAtRiskStudentsToExcelAsync(double gpaThreshold = 2.0)
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Department)
                .Where(s => s.IsActive && (s.GPA < gpaThreshold || s.CGPA < gpaThreshold))
                .OrderBy(s => s.GPA)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Riskli Öğrenciler");

            // Header
            worksheet.Cell(1, 1).Value = "Öğrenci No";
            worksheet.Cell(1, 2).Value = "Ad Soyad";
            worksheet.Cell(1, 3).Value = "E-posta";
            worksheet.Cell(1, 4).Value = "Bölüm";
            worksheet.Cell(1, 5).Value = "GPA";
            worksheet.Cell(1, 6).Value = "CGPA";
            worksheet.Cell(1, 7).Value = "Risk Durumu";

            var headerRange = worksheet.Range(1, 1, 1, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.Red;
            headerRange.Style.Font.FontColor = XLColor.White;

            for (int i = 0; i < students.Count; i++)
            {
                var student = students[i];
                var riskLevel = student.GPA < 1.5 ? "Kritik" :
                               student.GPA < 2.0 ? "Yüksek" :
                               student.CGPA < 2.0 ? "Orta" : "Düşük";

                worksheet.Cell(i + 2, 1).Value = student.StudentNumber;
                worksheet.Cell(i + 2, 2).Value = student.User?.FullName ?? "";
                worksheet.Cell(i + 2, 3).Value = student.User?.Email ?? "";
                worksheet.Cell(i + 2, 4).Value = student.Department?.Name ?? "";
                worksheet.Cell(i + 2, 5).Value = student.GPA;
                worksheet.Cell(i + 2, 6).Value = student.CGPA;
                worksheet.Cell(i + 2, 7).Value = riskLevel;

                // Risk seviyesine göre renklendirme
                if (riskLevel == "Kritik")
                {
                    worksheet.Row(i + 2).Style.Fill.BackgroundColor = XLColor.LightCoral;
                }
                else if (riskLevel == "Yüksek")
                {
                    worksheet.Row(i + 2).Style.Fill.BackgroundColor = XLColor.LightSalmon;
                }
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
