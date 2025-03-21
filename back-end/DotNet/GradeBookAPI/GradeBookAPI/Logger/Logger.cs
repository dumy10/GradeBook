using System.Reflection;

namespace GradeBookAPI.Logger
{
    public class Logger
    {
        private static readonly object _lock = new();
        private static Logger? _instance = null;
        private static string _logFile = string.Empty;

        public static Logger Instance => _instance ??= new Logger();

        private Logger()
        {
            string executingAssemblyPath = Assembly.GetExecutingAssembly().Location;

            // Get the directory where the executable is located
            executingAssemblyPath = Path.GetDirectoryName(executingAssemblyPath) ?? string.Empty;

            string logDirectory = Path.Combine(executingAssemblyPath, "ImagesProcessorLogs");

            // Create a log file for the current execution of the program in order to avoid overwriting logs across different executions
            _logFile = Path.Combine(logDirectory, $"GradeBookAPI-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            if (!File.Exists(_logFile))
            {
                File.Create(_logFile).Dispose();
            }

            LogMessage("Logger initialized.");
        }

        public void LogMessage(string message) => WriteLog("INFO", message);

        public void LogWarning(string message) => WriteLog("WARN", message);

        public void LogError(string message) => WriteLog("ERROR", message);

        private static void WriteLog(string level, string message)
        {
            if (string.IsNullOrWhiteSpace(_logFile))
            {
                throw new InvalidOperationException("Log file path is not set.");
            }

            lock (_lock)
            {
                using StreamWriter writer = new(_logFile, true) { AutoFlush = true };
                writer.WriteLine($"{{{level}}} {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
            }
        }

    }
}
