using GradeBookAPI.DTOs.AuthDTOs;
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
    [Authorize] // We need authentication for all endpoints in this controller
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                if (!AuthHelper.IsAuthenticated(HttpContext, out int userId))
                {
                    var auditLog = new AuditLog
                    {
                        UserId = 1,
                        Action = "GetProfile",
                        EntityType = "User",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "Unauthorized access to profile" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);
                    return Unauthorized(new { Success = false, Message = "Unauthorized" });
                }

                var profile = await _userService.GetUserProfileAsync(userId);

                if (profile == null)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = userId,
                        Action = "GetProfile",
                        EntityType = "User",
                        EntityId = userId,
                        Details = JsonSerializer.Serialize(new { message = "User not found", userId }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);
                    return NotFound(new { Success = false, Message = "User not found" });
                }

                var auditLogSuccess = new AuditLog
                {
                    UserId = userId,
                    Action = "GetProfile",
                    EntityType = "User",
                    EntityId = userId,
                    Details = JsonSerializer.Serialize(new { message = $"Profile retrieved for {userId}" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };

                GradeLogger.Instance.LogMessage(auditLogSuccess);

                return Ok(new { Success = true, Profile = profile });
            }
            catch (Exception ex)
            {
                var auditLog = new AuditLog
                {
                    UserId = 1,
                    Action = "GetProfile",
                    EntityType = "User",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = "An error occurred while retrieving the profile" });
            }
        }

        [HttpGet("profile/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserDetails(int userId)
        {
            try
            {
                if (!AuthHelper.IsAuthenticated(HttpContext, out int authenticatedUser))
                {
                    var auditLog = new AuditLog
                    {
                        UserId = authenticatedUser,
                        Action = "GetUserDetails",
                        EntityType = "User",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "Unauthorized access to user details" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);
                    return Unauthorized(new { Success = false, Message = "Unauthorized" });
                }

                var userDetails = await _userService.GetUserDetailsAsync(userId);

                if (userDetails == null)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = authenticatedUser,
                        Action = "GetUserDetails",
                        EntityType = "User",
                        EntityId = authenticatedUser,
                        Details = JsonSerializer.Serialize(new { message = "User not found", userId }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);
                    return NotFound(new { Success = false, Message = "User not found" });
                }

                var auditLogSuccess = new AuditLog
                {
                    UserId = authenticatedUser,
                    Action = "GetUserDetails",
                    EntityType = "User",
                    EntityId = authenticatedUser,
                    Details = JsonSerializer.Serialize(new { message = $"User details retrieved for {userId} by {authenticatedUser}" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };

                GradeLogger.Instance.LogMessage(auditLogSuccess);

                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                var errorLog = new AuditLog
                {
                    UserId = 1,
                    Action = "GetUserDetails",
                    EntityType = "User",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(errorLog);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = "An error occurred while retrieving the user details" });
            }
        }

        [HttpGet("profile/all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsersDetails()
        {
            try
            {
                if (!AuthHelper.IsAuthenticated(HttpContext, out int userId))
                {
                    var auditLog = new AuditLog
                    {
                        UserId = userId,
                        Action = "GetAllUsersDetails",
                        EntityType = "User",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "Unauthorized access to all users details" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);
                    return Unauthorized(new { Success = false, Message = "Unauthorized" });
                }
                var userDetails = await _userService.GetAllUsersDetailsAsync();
                if (userDetails == null)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = userId,
                        Action = "GetAllUsersDetails",
                        EntityType = "User",
                        EntityId = userId,
                        Details = JsonSerializer.Serialize(new { message = "No users found" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);
                    return NotFound(new { Success = false, Message = "No users found" });
                }
                var auditLogSuccess = new AuditLog
                {
                    UserId = userId,
                    Action = "GetAllUsersDetails",
                    EntityType = "User",
                    EntityId = userId,
                    Details = JsonSerializer.Serialize(new { message = $"All users details retrieved by {userId}" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogMessage(auditLogSuccess);
                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                var errorLog = new AuditLog
                {
                    UserId = 1,
                    Action = "GetAllUsersDetails",
                    EntityType = "User",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(errorLog);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = "An error occurred while retrieving all users details" });
            }
        }

        [HttpPut("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = 1,
                        Action = "UpdateProfile",
                        EntityType = "User",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "Invalid input data received for UpdateProfile" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);
                    return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
                }

                if (!AuthHelper.IsAuthenticated(HttpContext, out int userId))
                {
                    var auditLog = new AuditLog
                    {
                        UserId = 1,
                        Action = "UpdateProfile",
                        EntityType = "User",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "Unauthorized access to profile" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);
                    return Unauthorized(new { Success = false, Message = "Unauthorized" });
                }

                var result = await _userService.UpdateUserProfileAsync(userId, request);

                if (result)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = userId,
                        Action = "UpdateProfile",
                        EntityType = "User",
                        EntityId = userId,
                        Details = JsonSerializer.Serialize(new { message = $"Profile updated for {userId}" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };

                    GradeLogger.Instance.LogMessage(auditLog);

                    return Ok(new { Success = true, Message = "Profile updated successfully" });
                }

                var auditLogFailed = new AuditLog
                {
                    UserId = userId,
                    Action = "UpdateProfile",
                    EntityType = "User",
                    EntityId = userId,
                    Details = JsonSerializer.Serialize(new { message = $"Failed to update profile for {userId}" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLogFailed);
                return BadRequest(new { Success = false, Message = "Failed to update profile" });
            }
            catch (Exception ex)
            {
                var auditLog = new AuditLog
                {
                    UserId = 1,
                    Action = "UpdateProfile",
                    EntityType = "User",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = "An error occurred while updating the profile" });
            }
        }

        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = 1,
                        Action = "ChangePassword",
                        EntityType = "User",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "Invalid input data received for ChangePassword" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);
                    return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
                }

                if (!AuthHelper.IsAuthenticated(HttpContext, out int userId))
                {
                    var auditLog = new AuditLog
                    {
                        UserId = 1,
                        Action = "ChangePassword",
                        EntityType = "User",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "Unauthorized access to change password" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);

                    return Unauthorized(new { Success = false, Message = "Unauthorized" });
                }
                var result = await _userService.ChangePasswordAsync(userId, request);

                if (result)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = userId,
                        Action = "ChangePassword",
                        EntityType = "User",
                        EntityId = userId,
                        Details = JsonSerializer.Serialize(new { message = "Password changed successfully", userId = userId }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogMessage(auditLog);
                    return Ok(new { Success = true, Message = "Password changed successfully" });
                }

                var auditLogFailed = new AuditLog
                {
                    UserId = userId,
                    Action = "ChangePassword",
                    EntityType = "User",
                    EntityId = userId,
                    Details = JsonSerializer.Serialize(new { error = "Failed to change password", userId = userId }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLogFailed);
                return BadRequest(new { Success = false, Message = "Current password is incorrect" });
            }
            catch (Exception ex)
            {
                var auditLog = new AuditLog
                {
                    UserId = 1,
                    Action = "ChangePassword",
                    EntityType = "User",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = "An error occurred while changing the password" });
            }
        }
    }
}