using GradeBookAPI.Helpers;
using GradeBookAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GradeBookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BackupController : ControllerBase
    {
        private readonly IDatabaseBackupService _backupService;
        private readonly ILogger<BackupController> _logger;

        public BackupController(IDatabaseBackupService backupService, ILogger<BackupController> logger)
        {
            _backupService = backupService;
            _logger = logger;
        }

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBackup()
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                return Unauthorized(new { Success = false, Message = "Only teachers can create backups" });
            }

            try
            {
                var result = await _backupService.CreateBackupAsync();
                if (result)
                {
                    return Ok(new { Success = true, Message = "Database backup created successfully" });
                }
                else
                {
                    return StatusCode(500, new { Success = false, Message = "Failed to create database backup" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup");
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ListBackups()
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                return Unauthorized(new { Success = false, Message = "Only teachers can view backups" });
            }

            try
            {
                var backups = _backupService.GetAvailableBackups()
                    .Select(path => new FileInfo(path))
                    .Select(file => new
                    {
                        FileName = file.Name,
                        CreatedAt = file.CreationTime,
                        SizeInBytes = file.Length,
                        FullPath = file.FullName
                    })
                    .OrderByDescending(b => b.CreatedAt)
                    .ToList();

                return Ok(new { Success = true, Backups = backups });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing backups");
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }
        [HttpPost("restore/{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RestoreBackup(string fileName)
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                return Unauthorized(new { Success = false, Message = "Only teachers can restore backups" });
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest(new { Success = false, Message = "Backup file name is required" });
            }

            try
            {
                _logger.LogInformation($"Starting restore of backup file: {fileName}");

                var backups = _backupService.GetAvailableBackups();
                _logger.LogInformation($"Found {backups.Length} backup files");

                var backupPath = backups.FirstOrDefault(b => Path.GetFileName(b) == fileName);

                if (string.IsNullOrEmpty(backupPath))
                {
                    _logger.LogWarning($"Backup file not found: {fileName}");
                    return BadRequest(new { Success = false, Message = "Backup file not found" });
                }

                _logger.LogInformation($"Found backup at path: {backupPath}");

                // Set a timeout to prevent the process from hanging indefinitely
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

                var result = await _backupService.RestoreBackupAsync(backupPath);
                if (result)
                {
                    _logger.LogInformation("Database restored successfully");
                    return Ok(new { Success = true, Message = "Database restored successfully" });
                }
                else
                {
                    _logger.LogError("Failed to restore database");
                    return StatusCode(500, new { Success = false, Message = "Failed to restore database" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring backup");
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }
    }
}