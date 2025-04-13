using GradeBookAPI.Entities;
using GradeBookAPI.Services.Interfaces;
using System.Text.Json;

using GradeLogger = GradeBookAPI.Logger.Logger;

namespace GradeBookAPI.Services.BackgroundServices
{
    public class DatabaseBackupHostedService(IServiceProvider serviceProvider) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private Timer? _timer;

        private AuditLog AuditLog => new()
        {
            UserId = 1, // System
            EntityType = "DatabaseBackup",
            EntityId = 0,
            Action = "BackgroundService",
            Details = JsonSerializer.Serialize(new { message = "Database backup operation" }),
            IpAddress = "localhost",
            CreatedAt = DateTime.UtcNow
        };

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var startLog = AuditLog;
            startLog.Action = "ExecuteAsync";
            startLog.Details = JsonSerializer.Serialize(new { message = "Database Backup Service is starting." });
            startLog.CreatedAt = DateTime.UtcNow;
            GradeLogger.Instance.LogMessage(startLog);

            //Schedule initial backup
            ScheduleNextBackup();

            return Task.CompletedTask;
        }

        private void ScheduleNextBackup()
        {
            // Calculate time until next backup (3 AM)
            var now = DateTime.Now;
            var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 3, 0, 0);

            // If it's already past 3 AM, schedule for next day
            if (now > scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            var timeSpan = scheduledTime - now;

            var scheduleLog = AuditLog;
            scheduleLog.Action = "ScheduleNextBackup";
            scheduleLog.Details = JsonSerializer.Serialize(new { message = $"Next database backup scheduled at: {scheduledTime}" });
            scheduleLog.CreatedAt = DateTime.UtcNow;
            GradeLogger.Instance.LogMessage(scheduleLog);

            // Dispose existing timer if any
            _timer?.Dispose();

            // Create new timer
            _timer = new Timer(DoBackup, null, timeSpan, TimeSpan.FromHours(24));
        }

        private async void DoBackup(object? state)
        {
            var startLog = AuditLog;
            startLog.Action = "DoBackup";
            startLog.Details = JsonSerializer.Serialize(new { message = "Starting scheduled database backup" });
            startLog.CreatedAt = DateTime.UtcNow;
            GradeLogger.Instance.LogMessage(startLog);

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var backupService = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();
                    var result = await backupService.CreateBackupAsync();

                    if (result)
                    {
                        var successLog = AuditLog;
                        successLog.Action = "DoBackup";
                        successLog.Details = JsonSerializer.Serialize(new { message = "Scheduled database backup completed successfully" });
                        successLog.CreatedAt = DateTime.UtcNow;
                        GradeLogger.Instance.LogMessage(successLog);
                    }
                    else
                    {
                        var failLog = AuditLog;
                        failLog.Action = "DoBackup";
                        failLog.Details = JsonSerializer.Serialize(new { message = "Scheduled database backup failed" });
                        failLog.CreatedAt = DateTime.UtcNow;
                        GradeLogger.Instance.LogError(failLog);
                    }
                }

                // Reschedule next backup to prevent drift
                ScheduleNextBackup();
            }
            catch (Exception ex)
            {
                var errorLog = AuditLog;
                errorLog.Action = "DoBackup";
                errorLog.Details = JsonSerializer.Serialize(new { message = "An error occurred during scheduled database backup", error = ex.Message });
                errorLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(errorLog);

                // If backup fails, try again in 1 hour
                _timer?.Change(TimeSpan.FromHours(1), TimeSpan.FromHours(24));
            }
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            var stopLog = AuditLog;
            stopLog.Action = "StopAsync";
            stopLog.Details = JsonSerializer.Serialize(new { message = "Database Backup Service is stopping." });
            stopLog.CreatedAt = DateTime.UtcNow;
            GradeLogger.Instance.LogMessage(stopLog);

            _timer?.Change(Timeout.Infinite, 0);

            return base.StopAsync(stoppingToken);
        }

        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
}