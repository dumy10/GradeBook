using GradeBookAPI.DTOs.AuthDTOs;
using GradeBookAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                string? nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (nameIdentifier == null)
                {
                    GradeLogger.Instance.LogError("Unauthorized access to profile");
                    return Unauthorized(new { Success = false, Message = "Unauthorized" });
                }

                var userId = int.Parse(nameIdentifier);
                var profile = await _userService.GetUserProfileAsync(userId);

                if (profile == null)
                {
                    GradeLogger.Instance.LogError($"User not found for {userId}");
                    return NotFound(new { Success = false, Message = "User not found" });
                }

                GradeLogger.Instance.LogMessage($"Profile retrieved for {profile.Email}");
                return Ok(new { Success = true, Profile = profile });
            }
            catch (Exception ex)
            {
                GradeLogger.Instance.LogError($"Error retrieving profile: {ex.Message}");
                GradeLogger.Instance.LogError($"Stack trace: {ex.StackTrace}");
                GradeLogger.Instance.LogError($"Inner exception: {ex.InnerException?.Message}");

                return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = "An error occurred while retrieving the profile" });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    GradeLogger.Instance.LogError("Invalid input data received for UpdateProfile");
                    return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
                }

                string? nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (nameIdentifier == null)
                {
                    GradeLogger.Instance.LogError("Unauthorized access to profile");
                    return Unauthorized(new { Success = false, Message = "Unauthorized" });
                }

                var userId = int.Parse(nameIdentifier);
                var result = await _userService.UpdateUserProfileAsync(userId, request);

                if (result)
                {
                    GradeLogger.Instance.LogMessage($"Profile updated for {userId}");
                    return Ok(new { Success = true, Message = "Profile updated successfully" });
                }

                GradeLogger.Instance.LogError($"Failed to update profile for {userId}");
                return BadRequest(new { Success = false, Message = "Failed to update profile" });
            }
            catch (Exception ex)
            {
                GradeLogger.Instance.LogError($"Error updating profile: {ex.Message}");
                GradeLogger.Instance.LogError($"Stack trace: {ex.StackTrace}");
                GradeLogger.Instance.LogError($"Inner exception: {ex.InnerException?.Message}");

                return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = "An error occurred while updating the profile" });
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    GradeLogger.Instance.LogError("Invalid input data received for ChangePassword");
                    return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
                }

                string? nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (nameIdentifier == null)
                {
                    GradeLogger.Instance.LogError("Unauthorized access to profile");
                    return Unauthorized(new { Success = false, Message = "Unauthorized" });
                }

                var userId = int.Parse(nameIdentifier);
                var result = await _userService.ChangePasswordAsync(userId, request);

                if (result)
                {
                    GradeLogger.Instance.LogMessage($"Password changed for {userId}");
                    return Ok(new { Success = true, Message = "Password changed successfully" });
                }

                GradeLogger.Instance.LogError($"Failed to change password for {userId}");
                return BadRequest(new { Success = false, Message = "Current password is incorrect" });
            }
            catch (Exception ex)
            {
                GradeLogger.Instance.LogError($"Error changing password: {ex.Message}");
                GradeLogger.Instance.LogError($"Stack trace: {ex.StackTrace}");
                GradeLogger.Instance.LogError($"Inner exception: {ex.InnerException?.Message}");

                return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = "An error occurred while changing the password" });
            }
        }
    }
}