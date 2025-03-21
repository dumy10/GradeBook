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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = 1,
                        Action = "Register",
                        EntityType = "Auth",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "Invalid input data received for Register" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);
                    return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
                }

                var result = await _authService.RegisterAsync(request);

                if (result.Success)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = 1,
                        Action = "Register",
                        EntityType = "Auth",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "User registered successfully", email = result.Email }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogMessage(auditLog);
                    return Ok(result);
                }

                var auditLogError = new AuditLog
                {
                    UserId = 1,
                    Action = "Register",
                    EntityType = "Auth",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { error = result.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLogError);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                var auditLogEx = new AuditLog
                {
                    UserId = 1,
                    Action = "Register",
                    EntityType = "Auth",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };

                GradeLogger.Instance.LogError(auditLogEx);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "An error occurred during registration",
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = 1,
                        Action = "Login",
                        EntityType = "Auth",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "Invalid input data received for Login" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);
                    return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
                }

                var result = await _authService.LoginAsync(request);

                if (result.Success)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = 1,
                        Action = "Login",
                        EntityType = "Auth",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "User logged in successfully", email = result.Email }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogMessage(auditLog);
                    return Ok(result);
                }

                var auditLogError = new AuditLog
                {
                    UserId = 1,
                    Action = "Login",
                    EntityType = "Auth",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { error = result.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLogError);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                var auditLogEx = new AuditLog
                {
                    UserId = 1,
                    Action = "Login",
                    EntityType = "Auth",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLogEx);

                return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = "An error occurred during login" });
            }
        }

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                bool canConnect = await _context.Database.CanConnectAsync();

                if (canConnect)
                {
                    var userCount = await _context.Users.CountAsync();

                    var auditLog = new AuditLog
                    {
                        UserId = 1,
                        Action = "TestConnection",
                        EntityType = "Database",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "Database connection successful" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };

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
                    var auditLog = new AuditLog
                    {
                        UserId = 1,
                        Action = "TestConnection",
                        EntityType = "Database",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "Could not connect to the database" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
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
                var auditLogEx = new AuditLog
                {
                    UserId = 1,
                    Action = "TestConnection",
                    EntityType = "Database",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
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
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                var auditLog = new AuditLog
                {
                    UserId = 1,
                    Action = "ForgotPassword",
                    EntityType = "Auth",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Invalid input data received for ForgotPassword" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return BadRequest(new { Success = false, Message = "Invalid email format" });
            }

            await _authService.ForgotPasswordAsync(request.Email);

            var auditLogSuccess = new AuditLog
            {
                UserId = 1,
                Action = "ForgotPassword",
                EntityType = "Auth",
                EntityId = 0,
                Details = JsonSerializer.Serialize(new { message = "Password reset link sent", email = request.Email }),
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                CreatedAt = DateTime.UtcNow
            };
            GradeLogger.Instance.LogMessage(auditLogSuccess);

            // Always return success to prevent email enumeration
            return Ok(new { Success = true, Message = "If the email exists, a password reset link has been sent" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                var auditLog = new AuditLog
                {
                    UserId = 1,
                    Action = "ResetPassword",
                    EntityType = "Auth",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Invalid input data received for ResetPassword" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return BadRequest(new { Success = false, Message = "Invalid input" });
            }

            var result = await _authService.ResetPasswordAsync(request);

            if (result)
            {
                var auditLogSuccess = new AuditLog
                {
                    UserId = 1,
                    Action = "ResetPassword",
                    EntityType = "Auth",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Password reset successful" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogMessage(auditLogSuccess);
                return Ok(new { Success = true, Message = "Password reset successful" });
            }

            var auditLogError = new AuditLog
            {
                UserId = 1,
                Action = "ResetPassword",
                EntityType = "Auth",
                EntityId = 0,
                Details = JsonSerializer.Serialize(new { error = "Password reset failed" }),
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                CreatedAt = DateTime.UtcNow
            };
            GradeLogger.Instance.LogError(auditLogError);
            return BadRequest(new { Success = false, Message = "Invalid or expired token" });
        }
    }
}