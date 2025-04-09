using GradeBookAPI.Services.Interfaces;
using System.Diagnostics;

namespace GradeBookAPI.Services.Concretes
{
    public class DatabaseBackupService : IDatabaseBackupService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseBackupService> _logger;
        private readonly string _backupPath;
        private readonly int _retentionDays;

        public DatabaseBackupService(IConfiguration configuration, ILogger<DatabaseBackupService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Get backup configuration from settings
            _backupPath = _configuration["DatabaseBackup:BackupPath"] ?? "DatabaseBackups";
            _retentionDays = int.Parse(_configuration["DatabaseBackup:RetentionDays"] ?? "7");

            // Ensure backup directory exists
            if (!Directory.Exists(_backupPath))
            {
                Directory.CreateDirectory(_backupPath);
            }
        }

        public async Task<bool> CreateBackupAsync()
        {
            try
            {
                var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
                var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
                var dbName = Environment.GetEnvironmentVariable("DB_NAME");
                var dbUsername = Environment.GetEnvironmentVariable("DB_USERNAME");
                var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

                // Log database connection details (except password)
                _logger.LogInformation($"Creating backup using: Host={dbHost}, Port={dbPort}, DB={dbName}, User={dbUsername}");

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"{dbName}_{timestamp}.sql";
                var backupFilePath = Path.Combine(_backupPath, backupFileName);

                // Create PostgreSQL dump process
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "pg_dump",
                        Arguments = $"-h {dbHost} -p {dbPort} -U {dbUsername} -F c -b -v -f \"{backupFilePath}\" {dbName}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    // Set PostgreSQL password as environment variable
                    process.StartInfo.EnvironmentVariables["PGPASSWORD"] = dbPassword;

                    _logger.LogInformation($"Starting pg_dump: {process.StartInfo.FileName} {process.StartInfo.Arguments}");

                    // Check if pg_dump exists
                    if (!IsCommandAvailable("pg_dump"))
                    {
                        _logger.LogError("pg_dump command not found. Please install PostgreSQL client tools.");
                        return false;
                    }

                    // Start the process (important!)
                    if (!process.Start())
                    {
                        _logger.LogError("Failed to start pg_dump process");
                        return false;
                    }

                    // Read output and error streams to prevent deadlocks
                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();

                    // Wait for process to complete
                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0)
                    {
                        _logger.LogInformation($"Database backup completed successfully: {backupFilePath}");
                        await CleanupOldBackupsAsync();
                        return true;
                    }
                    else
                    {
                        _logger.LogError($"pg_dump failed with exit code {process.ExitCode}. Error: {error}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating database backup");
                return false;
            }
        }

        public async Task<bool> RestoreBackupAsync(string backupFilePath)
        {
            try
            {
                if (!File.Exists(backupFilePath))
                {
                    _logger.LogError($"Backup file not found: {backupFilePath}");
                    return false;
                }

                var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
                var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
                var dbName = Environment.GetEnvironmentVariable("DB_NAME");
                var dbUsername = Environment.GetEnvironmentVariable("DB_USERNAME");
                var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

                _logger.LogInformation($"Restoring backup from {backupFilePath} to {dbName}");

                // First, try to close all existing connections to the database
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "psql",
                        Arguments = $"-h {dbHost} -p {dbPort} -U {dbUsername} -d postgres -c \"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{dbName}' AND pid <> pg_backend_pid();\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    process.StartInfo.EnvironmentVariables["PGPASSWORD"] = dbPassword;

                    try
                    {
                        process.Start();
                        await process.WaitForExitAsync();
                        _logger.LogInformation("Successfully closed existing database connections");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Could not terminate existing connections: {ex.Message}");
                        // Continue anyway
                    }
                }

                // Create PostgreSQL restore process
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "pg_restore",
                        // Use --clean to drop objects before recreating them
                        // Use --no-owner to skip ownership commands that might fail
                        Arguments = $"-h {dbHost} -p {dbPort} -U {dbUsername} -d {dbName} --clean --no-owner -v \"{backupFilePath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    // Set PostgreSQL password as environment variable
                    process.StartInfo.EnvironmentVariables["PGPASSWORD"] = dbPassword;

                    _logger.LogInformation($"Starting pg_restore with command: {process.StartInfo.FileName} {process.StartInfo.Arguments}");

                    // Check if pg_restore exists
                    if (!IsCommandAvailable("pg_restore"))
                    {
                        _logger.LogError("pg_restore command not found. Please install PostgreSQL client tools.");
                        return false;
                    }

                    // Start the process
                    if (!process.Start())
                    {
                        _logger.LogError("Failed to start pg_restore process");
                        return false;
                    }

                    // Read output and error streams to prevent deadlocks
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    // Wait for process with a timeout
                    var processExited = process.WaitForExit(300000); // 5 minute timeout

                    if (!processExited)
                    {
                        try
                        {
                            process.Kill();
                            _logger.LogError("pg_restore process timed out after 5 minutes and was killed");
                            return false;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to kill pg_restore process after timeout");
                            return false;
                        }
                    }

                    var output = await outputTask;
                    var error = await errorTask;

                    // Log detailed output regardless of success
                    _logger.LogInformation($"pg_restore standard output: {output}");
                    if (!string.IsNullOrEmpty(error))
                    {
                        _logger.LogWarning($"pg_restore standard error: {error}");
                    }

                    // Some warnings in stderr are normal, so check exit code instead
                    if (process.ExitCode == 0)
                    {
                        _logger.LogInformation($"Database restore completed successfully from {backupFilePath}");
                        return true;
                    }
                    else
                    {
                        _logger.LogError($"pg_restore failed with exit code {process.ExitCode}. Error: {error}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while restoring database backup");
                return false;
            }
        }

        private async Task CleanupOldBackupsAsync()
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-_retentionDays);
                var backupFiles = Directory.GetFiles(_backupPath, "*.sql");

                foreach (var file in backupFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        _logger.LogInformation($"Deleted old backup file: {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while cleaning up old backups");
            }
        }

        public string[] GetAvailableBackups()
        {
            return Directory.GetFiles(_backupPath, "*.sql");
        }

        private bool IsCommandAvailable(string command)
        {
            try
            {
                using (var process = new Process())
                {
                    // For Windows
                    if (OperatingSystem.IsWindows())
                    {
                        process.StartInfo = new ProcessStartInfo
                        {
                            FileName = "where",
                            Arguments = command,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                    }
                    // For Linux/macOS
                    else
                    {
                        process.StartInfo = new ProcessStartInfo
                        {
                            FileName = "which",
                            Arguments = command,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                    }

                    process.Start();
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    return process.ExitCode == 0 && !string.IsNullOrEmpty(output);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}