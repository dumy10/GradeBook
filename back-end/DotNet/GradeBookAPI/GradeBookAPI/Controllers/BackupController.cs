using GradeBookAPI.Entities;
using GradeBookAPI.Helpers;
using GradeBookAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

using GradeLogger = GradeBookAPI.Logger.Logger;

namespace GradeBookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BackupController(IDatabaseBackupService backupService) : ControllerBase
    {
        private readonly IDatabaseBackupService _backupService = backupService;

        private AuditLog AuditLog => new()
        {
            UserId = 1,
            EntityType = "Backup",
            EntityId = 0,
            Action = "BackupController",
            Details = JsonSerializer.Serialize(new { message = "BackupController initialized" }),
            IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            CreatedAt = DateTime.UtcNow
        };

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBackup()
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = AuditLog;
                auditLog.UserId = teacherId;
                auditLog.Action = "CreateBackup";
                auditLog.Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt to create backup" });
                auditLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Only teachers can create backups" });
            }

            try
            {
                var result = await _backupService.CreateBackupAsync();
                if (result)
                {
                    var auditLog = AuditLog;
                    auditLog.UserId = teacherId;
                    auditLog.Action = "CreateBackup";
                    auditLog.Details = JsonSerializer.Serialize(new { message = "Database backup created successfully" });
                    auditLog.CreatedAt = DateTime.UtcNow;
                    GradeLogger.Instance.LogMessage(auditLog);

                    return Ok(new { Success = true, Message = "Database backup created successfully" });
                }
                else
                {
                    var auditLog = AuditLog;
                    auditLog.UserId = teacherId;
                    auditLog.Action = "CreateBackup";
                    auditLog.Details = JsonSerializer.Serialize(new { message = "Failed to create database backup" });
                    auditLog.CreatedAt = DateTime.UtcNow;
                    GradeLogger.Instance.LogError(auditLog);

                    return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = "Failed to create database backup" });
                }
            }
            catch (Exception ex)
            {
                var auditLog = AuditLog;
                auditLog.UserId = teacherId;
                auditLog.Action = "CreateBackup";
                auditLog.Details = JsonSerializer.Serialize(new { message = $"Error creating backup: {ex.Message}" });
                auditLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLog);

                return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ListBackups()
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = AuditLog;
                auditLog.UserId = teacherId;
                auditLog.Action = "ListBackups";
                auditLog.Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt to list backups" });
                auditLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLog);

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

                var auditLog = AuditLog;
                auditLog.UserId = teacherId;
                auditLog.Action = "ListBackups";
                auditLog.Details = JsonSerializer.Serialize(new { message = $"Listed {backups.Count} backups" });
                auditLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogMessage(auditLog);

                return Ok(new { Success = true, Backups = backups });
            }
            catch (Exception ex)
            {
                var auditLog = AuditLog;
                auditLog.UserId = teacherId;
                auditLog.Action = "ListBackups";
                auditLog.Details = JsonSerializer.Serialize(new { message = $"Error listing backups: {ex.Message}" });
                auditLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLog);

                return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = $"Error: {ex.Message}" });
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
                var auditLog = AuditLog;
                auditLog.UserId = teacherId;
                auditLog.Action = "RestoreBackup";
                auditLog.Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt to restore backup" });
                auditLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Only teachers can restore backups" });
            }

            if (string.IsNullOrEmpty(fileName))
            {
                var auditLog = AuditLog;
                auditLog.UserId = teacherId;
                auditLog.Action = "RestoreBackup";
                auditLog.Details = JsonSerializer.Serialize(new { message = "Backup file name is required" });
                auditLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogWarning(auditLog);

                return BadRequest(new { Success = false, Message = "Backup file name is required" });
            }

            try
            {
                var startLog = AuditLog;
                startLog.UserId = teacherId;
                startLog.Action = "RestoreBackup";
                startLog.Details = JsonSerializer.Serialize(new { message = $"Starting restore of backup file: {fileName}" });
                startLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogMessage(startLog);

                var backups = _backupService.GetAvailableBackups();

                var backupsLog = AuditLog;
                backupsLog.UserId = teacherId;
                backupsLog.Action = "RestoreBackup";
                backupsLog.Details = JsonSerializer.Serialize(new { message = $"Found {backups.Length} backup files" });
                backupsLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogMessage(backupsLog);

                var backupPath = backups.FirstOrDefault(b => Path.GetFileName(b) == fileName);

                if (string.IsNullOrEmpty(backupPath))
                {
                    var notFoundLog = AuditLog;
                    notFoundLog.UserId = teacherId;
                    notFoundLog.Action = "RestoreBackup";
                    notFoundLog.Details = JsonSerializer.Serialize(new { message = $"Backup file not found: {fileName}" });
                    notFoundLog.CreatedAt = DateTime.UtcNow;
                    GradeLogger.Instance.LogWarning(notFoundLog);

                    return BadRequest(new { Success = false, Message = "Backup file not found" });
                }

                var pathLog = AuditLog;
                pathLog.UserId = teacherId;
                pathLog.Action = "RestoreBackup";
                pathLog.Details = JsonSerializer.Serialize(new { message = $"Found backup at path: {backupPath}" });
                pathLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogMessage(pathLog);

                // Set a timeout to prevent the process from hanging indefinitely
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

                var result = await _backupService.RestoreBackupAsync(backupPath);
                if (result)
                {
                    var successLog = AuditLog;
                    successLog.UserId = teacherId;
                    successLog.Action = "RestoreBackup";
                    successLog.Details = JsonSerializer.Serialize(new { message = "Database restored successfully" });
                    successLog.CreatedAt = DateTime.UtcNow;
                    GradeLogger.Instance.LogMessage(successLog);

                    return Ok(new { Success = true, Message = "Database restored successfully" });
                }
                else
                {
                    var failLog = AuditLog;
                    failLog.UserId = teacherId;
                    failLog.Action = "RestoreBackup";
                    failLog.Details = JsonSerializer.Serialize(new { message = "Failed to restore database" });
                    failLog.CreatedAt = DateTime.UtcNow;
                    GradeLogger.Instance.LogError(failLog);

                    return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = "Failed to restore database" });
                }
            }
            catch (Exception ex)
            {
                var errorLog = AuditLog;
                errorLog.UserId = teacherId;
                errorLog.Action = "RestoreBackup";
                errorLog.Details = JsonSerializer.Serialize(new { message = $"Error restoring backup: {ex.Message}" });
                errorLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(errorLog);

                return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }
    }
}