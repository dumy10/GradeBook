using GradeBookAuthAPI.DTOs.AuthDTOs;
using GradeBookAuthAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GradeBookAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // We need authentication for all endpoints in this controller
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var profile = await _userService.GetUserProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new { Success = false, Message = "User not found" });
                }

                return Ok(new { Success = true, Profile = profile });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting profile: {ex.Message}");
                return StatusCode(500, new { Success = false, Message = "An error occurred while retrieving the profile" });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _userService.UpdateUserProfileAsync(userId, request);

                if (result)
                {
                    return Ok(new { Success = true, Message = "Profile updated successfully" });
                }

                return BadRequest(new { Success = false, Message = "Failed to update profile" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile: {ex.Message}");
                return StatusCode(500, new { Success = false, Message = "An error occurred while updating the profile" });
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _userService.ChangePasswordAsync(userId, request);

                if (result)
                {
                    return Ok(new { Success = true, Message = "Password changed successfully" });
                }

                return BadRequest(new { Success = false, Message = "Current password is incorrect" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing password: {ex.Message}");
                return StatusCode(500, new { Success = false, Message = "An error occurred while changing the password" });
            }
        }
    }
}