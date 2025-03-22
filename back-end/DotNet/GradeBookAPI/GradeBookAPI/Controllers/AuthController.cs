using GradeBookAPI.Data;
using GradeBookAPI.DTOs.AuthDTOs;
using GradeBookAPI.Entities;
using GradeBookAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using GradeLogger = GradeBookAPI.Logger.Logger;

namespace GradeBookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, AppDbContext context) : ControllerBase
    {
        private readonly IAuthService _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        private AuditLog AuditLog => new()
        {
            UserId = 1,
            EntityType = "Auth",
            EntityId = 0,
            Action = "AuthController",
            Details = JsonSerializer.Serialize(new { message = "AuthController initialized" }),
            IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            CreatedAt = DateTime.UtcNow
        };


        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var auditLog = AuditLog;
                    auditLog.Action = "Register";
                    auditLog.Details = JsonSerializer.Serialize(new { message = "Invalid input data received for Register" });
                    auditLog.CreatedAt = DateTime.UtcNow;
                    GradeLogger.Instance.LogError(auditLog);
                    return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
                }

                var result = await _authService.RegisterAsync(request);

                if (result.Success)
                {
                    var auditLog = AuditLog;
                    auditLog.Action = "Register";
                    auditLog.Details = JsonSerializer.Serialize(new { message = "User registered successfully", email = result.Email });
                    auditLog.CreatedAt = DateTime.UtcNow;
                    GradeLogger.Instance.LogMessage(auditLog);
                    return Ok(result);
                }

                var auditLogError = AuditLog;
                auditLogError.Action = "Register";
                auditLogError.Details = JsonSerializer.Serialize(new { error = result.Message });
                auditLogError.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLogError);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                var auditLogEx = AuditLog;
                auditLogEx.Action = "Register";
                auditLogEx.Details = JsonSerializer.Serialize(new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
                auditLogEx.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLogEx);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "An error occurred during registration",
                });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var auditLog = AuditLog;
                    auditLog.Action = "Login";
                    auditLog.Details = JsonSerializer.Serialize(new { message = "Invalid input data received for Login" });
                    auditLog.CreatedAt = DateTime.UtcNow;
                    GradeLogger.Instance.LogError(auditLog);
                    return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
                }

                var result = await _authService.LoginAsync(request);

                if (result.Success)
                {
                    var auditLog = AuditLog;
                    auditLog.Action = "Login";
                    auditLog.Details = JsonSerializer.Serialize(new { message = "User logged in successfully", email = result.Email });
                    auditLog.CreatedAt = DateTime.UtcNow;
                    GradeLogger.Instance.LogMessage(auditLog);
                    return Ok(result);
                }

                var auditLogError = AuditLog;
                auditLogError.Action = "Login";
                auditLogError.Details = JsonSerializer.Serialize(new { error = result.Message });
                auditLogError.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLogError);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                var auditLogEx = AuditLog;
                auditLogEx.Action = "Login";
                auditLogEx.Details = JsonSerializer.Serialize(new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
                auditLogEx.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLogEx);

                return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = "An error occurred during login" });
            }
        }

        [HttpGet("test-connection")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                bool canConnect = await _context.Database.CanConnectAsync();

                if (canConnect)
                {
                    var userCount = await _context.Users.CountAsync();

                    var auditLog = AuditLog;
                    auditLog.Action = "TestConnection";
                    auditLog.EntityType = "Database";
                    auditLog.Details = JsonSerializer.Serialize(new { message = "Database connection successful" });
                    auditLog.CreatedAt = DateTime.UtcNow;
                    GradeLogger.Instance.LogMessage(auditLog);

                    return Ok(new
                    {
                        Success = true,
                        Message = "Database connection successful",
                        UserCount = userCount
                    });
                }
                else
                {
                    var auditLog = AuditLog;
                    auditLog.Action = "TestConnection";
                    auditLog.EntityType = "Database";
                    auditLog.Details = JsonSerializer.Serialize(new { message = "Could not connect to the database" });
                    auditLog.CreatedAt = DateTime.UtcNow;
                    GradeLogger.Instance.LogError(auditLog);
                    return StatusCode(StatusCodes.Status500InternalServerError, new
                    {
                        Success = false,
                        Message = "Could not connect to the database"
                    });
                }
            }
            catch (Exception ex)
            {
                var auditLogEx = AuditLog;
                auditLogEx.Action = "TestConnection";
                auditLogEx.EntityType = "Database";
                auditLogEx.Details = JsonSerializer.Serialize(new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
                auditLogEx.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLogEx);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "Database connection failed",
                });
            }
        }

        // AuthController.cs
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                var auditLog = AuditLog;
                auditLog.Action = "ForgotPassword";
                auditLog.Details = JsonSerializer.Serialize(new { message = "Invalid input data received for ForgotPassword" });
                auditLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLog);
                return BadRequest(new { Success = false, Message = "Invalid email format" });
            }

            await _authService.ForgotPasswordAsync(request.Email);

            var auditLogSuccess = AuditLog;
            auditLogSuccess.Action = "ForgotPassword";
            auditLogSuccess.Details = JsonSerializer.Serialize(new { message = "Password reset link sent", email = request.Email });
            auditLogSuccess.CreatedAt = DateTime.UtcNow;
            GradeLogger.Instance.LogMessage(auditLogSuccess);

            // Always return success to prevent email enumeration
            return Ok(new { Success = true, Message = "If the email exists, a password reset link has been sent" });
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                var auditLog = AuditLog;
                auditLog.Action = "ResetPassword";
                auditLog.Details = JsonSerializer.Serialize(new { message = "Invalid input data received for ResetPassword" });
                auditLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLog);
                return BadRequest(new { Success = false, Message = "Invalid input" });
            }

            var result = await _authService.ResetPasswordAsync(request);

            if (result)
            {
                var auditLogSuccess = AuditLog;
                auditLogSuccess.Action = "ResetPassword";
                auditLogSuccess.Details = JsonSerializer.Serialize(new { message = "Password reset successful" });
                auditLogSuccess.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogMessage(auditLogSuccess);
                return Ok(new { Success = true, Message = "Password reset successful" });
            }

            var auditLogError = AuditLog;
            auditLogError.Action = "ResetPassword";
            auditLogError.Details = JsonSerializer.Serialize(new { error = "Password reset failed" });
            auditLogError.CreatedAt = DateTime.UtcNow;
            GradeLogger.Instance.LogError(auditLogError);
            return BadRequest(new { Success = false, Message = "Invalid or expired token" });
        }
    }
}