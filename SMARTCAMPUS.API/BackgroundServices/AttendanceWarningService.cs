using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs.Notifications;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.API.BackgroundServices
{
    /// <summary>
    /// Her gün %20+ devamsızlığı olan öğrencilere uyarı gönderen background service
    /// </summary>
    public class AttendanceWarningService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AttendanceWarningService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Her 24 saatte bir kontrol
        private readonly TimeSpan _dailyRunTime = TimeSpan.FromHours(8); // Saat 08:00'de çalış

        public AttendanceWarningService(
            IServiceProvider serviceProvider,
            ILogger<AttendanceWarningService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AttendanceWarningService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Bir sonraki çalışma zamanını hesapla
                    var now = DateTime.Now;
                    var nextRun = now.Date.Add(_dailyRunTime);
                    
                    if (now >= nextRun)
                    {
                        nextRun = nextRun.AddDays(1);
                    }

                    var delay = nextRun - now;
                    _logger.LogInformation("Next attendance check scheduled at: {NextRun}", nextRun);

                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await CheckAttendanceAndNotifyAsync();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Service is stopping, this is expected
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AttendanceWarningService");
                    // Wait a bit before retrying
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("AttendanceWarningService stopped.");
        }

        private async Task CheckAttendanceAndNotifyAsync()
        {
            _logger.LogInformation("Starting attendance check...");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CampusContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<IAdvancedNotificationService>();

            const double absenteeismThreshold = 20.0; // %20 devamsızlık eşiği

            // Tüm aktif öğrencileri al
            var students = await context.Students
                .Include(s => s.User)
                .Where(s => s.IsActive)
                .ToListAsync();

            var warningCount = 0;

            foreach (var student in students)
            {
                try
                {
                    // Öğrencinin kayıtlı olduğu dersleri al
                    var enrollments = await context.Enrollments
                        .Include(e => e.Section)
                        .Where(e => e.StudentId == student.Id && e.IsActive && e.Status == EnrollmentStatus.Enrolled)
                        .ToListAsync();

                    foreach (var enrollment in enrollments)
                    {
                        // Bu ders için toplam oturum sayısı
                        var totalSessions = await context.AttendanceSessions
                            .CountAsync(a => a.SectionId == enrollment.SectionId && a.IsActive);

                        if (totalSessions == 0) continue;

                        // Öğrencinin katıldığı oturum sayısı
                        var attendedSessions = await context.AttendanceRecords
                            .CountAsync(ar =>
                                ar.StudentId == student.Id &&
                                ar.Session.SectionId == enrollment.SectionId &&
                                ar.IsActive &&
                                !ar.IsFlagged);

                        // Devamsızlık oranını hesapla
                        var absenteeRate = ((double)(totalSessions - attendedSessions) / totalSessions) * 100;

                        if (absenteeRate >= absenteeismThreshold)
                        {
                            // Uyarı bildirimi gönder
                            await notificationService.SendNotificationAsync(new CreateNotificationDto
                            {
                                UserId = student.UserId,
                                Title = "Devamsızlık Uyarısı",
                                Message = $"{enrollment.Section?.Course?.Code ?? "Ders"} dersinde devamsızlık oranınız %{absenteeRate:F0}'e ulaştı. " +
                                          "Devamsızlık sınırını aşmanız durumunda dersten başarısız sayılabilirsiniz.",
                                Type = NotificationType.Warning,
                                Category = NotificationCategory.Attendance,
                                RelatedEntityType = "Enrollment",
                                RelatedEntityId = enrollment.Id
                            });

                            warningCount++;
                            _logger.LogInformation(
                                "Attendance warning sent to student {StudentId} for section {SectionId} (Absentee rate: {Rate}%)",
                                student.Id, enrollment.SectionId, absenteeRate);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing attendance for student {StudentId}", student.Id);
                }
            }

            _logger.LogInformation("Attendance check completed. Total warnings sent: {Count}", warningCount);
        }
    }
}
