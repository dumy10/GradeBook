using GradeBookAPI.Data;
using GradeBookAPI.Entities;
using System.Reflection;
using System.Text.Json;

namespace GradeBookAPI.Logger
{
    public class Logger
    {
        private static readonly object _lock = new();
        private static Logger? _instance = null;
        private static string _logFile = string.Empty;

        private static IServiceProvider? _serviceProvider;

        public static void InitializeServiceProvider(IServiceProvider provider)
        {
            _serviceProvider = provider;
        }

        public static Logger Instance => _instance ??= new Logger();

        private Logger()
        {
            string executingAssemblyPath = Assembly.GetExecutingAssembly().Location;
            executingAssemblyPath = Path.GetDirectoryName(executingAssemblyPath) ?? string.Empty;
            string logDirectory = Path.Combine(executingAssemblyPath, "GradeBookLogs");
            _logFile = Path.Combine(logDirectory, $"GradeBookAPI-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            if (!File.Exists(_logFile))
            {
                File.Create(_logFile).Dispose();
            }
            LogMessage(new AuditLog { EntityType = "Logger", Action = "Initialize", Details = JsonSerializer.Serialize(new { message = "Logger initialized." }), UserId = 1, EntityId = 0, IpAddress = "localhost", CreatedAt = DateTime.UtcNow });
        }

        public void LogMessage(AuditLog auditLog) => WriteLog("INFO", auditLog);
        public void LogWarning(AuditLog auditLog) => WriteLog("WARN", auditLog);
        public void LogError(AuditLog auditLog) => WriteLog("ERROR", auditLog);

        private static void WriteLog(string level, AuditLog auditLog)
        {
            if (string.IsNullOrWhiteSpace(_logFile))
            {
                throw new InvalidOperationException("Log file path is not set.");
            }

            lock (_lock)
            {
                using StreamWriter writer = new(_logFile, true) { AutoFlush = true };
                writer.WriteLine($"{{{level}}} {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {auditLog.EntityType} | {auditLog.Action} | {auditLog.Details}");
            }

            // Save auditLog to database by creating a scope and retrieving AppDbContext directly.
            try
            {
                if (_serviceProvider != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    context.AuditLogs.Add(auditLog);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    using StreamWriter writer = new(_logFile, true) { AutoFlush = true };
                    writer.WriteLine($"{{ERROR}} {DateTime.Now:yyyy-MM-dd HH:mm:ss} - Failed to save AuditLog to database: {ex.Message}");
                    writer.WriteLine($"{{ERROR}} {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {ex.InnerException?.Message}");
                }
            }
        }
    }
}
