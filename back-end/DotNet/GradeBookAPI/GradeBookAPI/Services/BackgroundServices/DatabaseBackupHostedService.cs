using GradeBookAPI.Services.Interfaces;

namespace GradeBookAPI.Services.BackgroundServices
{
    public class DatabaseBackupHostedService : BackgroundService
    {
        private readonly ILogger<DatabaseBackupHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        public DatabaseBackupHostedService(
            ILogger<DatabaseBackupHostedService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Database Backup Service is starting.");

            // Schedule initial backup
            //ScheduleNextBackup();

            // TEST MODE: Run immediately instead of at 3 AM
            _timer = new Timer(DoBackup, null, TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);


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
            _logger.LogInformation($"Next database backup scheduled at: {scheduledTime}");

            // Dispose existing timer if any
            _timer?.Dispose();

            // Create new timer
            _timer = new Timer(DoBackup, null, timeSpan, TimeSpan.FromHours(24));
        }

        private async void DoBackup(object? state)
        {
            _logger.LogInformation("Starting scheduled database backup");

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var backupService = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();
                    var result = await backupService.CreateBackupAsync();

                    if (result)
                    {
                        _logger.LogInformation("Scheduled database backup completed successfully");
                    }
                    else
                    {
                        _logger.LogError("Scheduled database backup failed");
                    }
                }

                // Reschedule next backup to prevent drift
                ScheduleNextBackup();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during scheduled database backup");

                // If backup fails, try again in 1 hour
                _timer?.Change(TimeSpan.FromHours(1), TimeSpan.FromHours(24));
            }
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Database Backup Service is stopping.");

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