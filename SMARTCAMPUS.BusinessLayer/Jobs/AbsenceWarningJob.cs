using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Constants;
using SMARTCAMPUS.EntityLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SMARTCAMPUS.BusinessLayer.Jobs
{
    public class AbsenceWarningJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AbsenceWarningJob> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Run daily

        public AbsenceWarningJob(IServiceProvider serviceProvider, ILogger<AbsenceWarningJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAbsenceWarningsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing absence warnings");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessAbsenceWarningsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CampusContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<SMARTCAMPUS.BusinessLayer.Abstract.INotificationService>();

            _logger.LogInformation("Starting absence warning job...");

            // Get all active enrollments
            var enrollments = await context.Enrollments
                .Where(e => e.Status == EnrollmentStatus.Active && e.IsActive)
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Section)
                    .ThenInclude(s => s.Course)
                .Include(e => e.Section)
                    .ThenInclude(s => s.Instructor)
                .ToListAsync();

            foreach (var enrollment in enrollments)
            {
                try
                {
                    // Get all sessions for this section
                    var sessions = await context.AttendanceSessions
                        .Where(s => s.SectionId == enrollment.SectionId && s.IsActive)
                        .Include(s => s.AttendanceRecords)
                        .ToListAsync();

                    var totalSessions = sessions.Count;
                    if (totalSessions == 0)
                        continue;

                    // Count attendance
                    var records = sessions
                        .SelectMany(s => s.AttendanceRecords ?? new List<AttendanceRecord>())
                        .Where(r => r.StudentId == enrollment.StudentId && r.CheckInTime.HasValue)
                        .ToList();

                    var presentCount = records.Count;
                    var attendancePercentage = (decimal)presentCount / totalSessions * 100;
                    var absencePercentage = 100 - attendancePercentage;

                    // Check thresholds
                    if (absencePercentage >= 30) // Critical: >= 30% absence
                    {
                        await SendCriticalWarningAsync(enrollment, attendancePercentage, emailService);
                        _logger.LogWarning($"Critical absence warning sent to student {enrollment.StudentId} for course {enrollment.Section.Course?.Code}");
                    }
                    else if (absencePercentage >= 20) // Warning: >= 20% absence
                    {
                        await SendWarningAsync(enrollment, attendancePercentage, emailService);
                        _logger.LogInformation($"Absence warning sent to student {enrollment.StudentId} for course {enrollment.Section.Course?.Code}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing absence warning for enrollment {enrollment.Id}");
                }
            }

            _logger.LogInformation("Absence warning job completed");
        }

        private async Task SendWarningAsync(
            Enrollment enrollment,
            decimal attendancePercentage,
            SMARTCAMPUS.BusinessLayer.Abstract.INotificationService emailService)
        {
            var studentEmail = enrollment.Student?.User?.Email;
            if (string.IsNullOrEmpty(studentEmail))
                return;

            var courseCode = enrollment.Section?.Course?.Code ?? "Unknown";
            var courseName = enrollment.Section?.Course?.Name ?? "Unknown Course";
            var subject = $"Absence Warning - {courseCode}";
            var message = $@"
Dear {enrollment.Student?.User?.FullName},

This is a warning regarding your attendance in {courseCode} - {courseName}.

Your current attendance rate is {attendancePercentage:F1}%, which is below the acceptable threshold.

Please ensure regular attendance to avoid academic consequences.

Best regards,
SmartCampus System
";

            // TODO: Send email
            // await emailService.SendEmailAsync(studentEmail, subject, message);

            // TODO: Send push notification
            // await _pushNotificationService.SendAsync(enrollment.StudentId, "Absence Warning", message);

            // TODO: Notify advisor
            // await _notificationService.NotifyAdvisorAsync(enrollment.StudentId, enrollment);
        }

        private async Task SendCriticalWarningAsync(
            Enrollment enrollment,
            decimal attendancePercentage,
            SMARTCAMPUS.BusinessLayer.Abstract.INotificationService emailService)
        {
            var studentEmail = enrollment.Student?.User?.Email;
            if (string.IsNullOrEmpty(studentEmail))
                return;

            var courseCode = enrollment.Section?.Course?.Code ?? "Unknown";
            var courseName = enrollment.Section?.Course?.Name ?? "Unknown Course";
            var subject = $"CRITICAL: Absence Warning - {courseCode}";
            var message = $@"
Dear {enrollment.Student?.User?.FullName},

This is a CRITICAL warning regarding your attendance in {courseCode} - {courseName}.

Your current attendance rate is {attendancePercentage:F1}%, which is critically low.

Immediate action is required. Please contact your instructor and academic advisor.

Best regards,
SmartCampus System
";

            // TODO: Send email
            // await emailService.SendEmailAsync(studentEmail, subject, message);

            // TODO: Send SMS (optional)
            // await _smsService.SendSmsAsync(enrollment.Student.PhoneNumber, message);

            // TODO: Send push notification
            // await _pushNotificationService.SendAsync(enrollment.StudentId, "Critical Absence Warning", message);

            // TODO: Notify advisor
            // await _notificationService.NotifyAdvisorAsync(enrollment.StudentId, enrollment);
        }
    }
}

