using GradeBookAPI.Data;
using GradeBookAPI.DTOs.AuthDTOs;
using GradeBookAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                    GradeLogger.Instance.LogError("Invalid input data received for Register");
                    return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
                }

                var result = await _authService.RegisterAsync(request);

                if (result.Success)
                {
                    GradeLogger.Instance.LogMessage($"User {result.Email} registered successfully");
                    return Ok(result);
                }

                GradeLogger.Instance.LogError($"Registration error: {result.Message}");
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                GradeLogger.Instance.LogError($"Registration error: {ex.Message}");
                GradeLogger.Instance.LogError($"Stack trace: {ex.StackTrace}");
                GradeLogger.Instance.LogError($"Inner exception: {ex.InnerException?.Message}");

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
                    GradeLogger.Instance.LogError("Invalid input data received for Login");
                    return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
                }

                var result = await _authService.LoginAsync(request);

                if (result.Success)
                {
                    GradeLogger.Instance.LogMessage($"User {result.Email} logged in successfully");
                    return Ok(result);
                }

                GradeLogger.Instance.LogError($"Login error: {result.Message}");
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                GradeLogger.Instance.LogError($"Login error: {ex.Message}");
                GradeLogger.Instance.LogError($"Stack trace: {ex.StackTrace}");
                GradeLogger.Instance.LogError($"Inner exception: {ex.InnerException?.Message}");

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

                    return Ok(new
                    {
                        Success = true,
                        Message = "Database connection successful",
                        UserCount = userCount
                    });
                }
                else
                {
                    GradeLogger.Instance.LogError("Could not connect to the database");
                    return StatusCode(StatusCodes.Status500InternalServerError, new
                    {
                        Success = false,
                        Message = "Could not connect to the database"
                    });
                }
            }
            catch (Exception ex)
            {
                GradeLogger.Instance.LogError($"Database connection error: {ex.Message}");
                GradeLogger.Instance.LogError($"Stack trace: {ex.StackTrace}");
                GradeLogger.Instance.LogError($"Inner exception: {ex.InnerException?.Message}");

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
                GradeLogger.Instance.LogError("Invalid input data received for ForgotPassword");
                return BadRequest(new { Success = false, Message = "Invalid email format" });
            }

            await _authService.ForgotPasswordAsync(request.Email);

            GradeLogger.Instance.LogMessage($"Password reset link sent to {request.Email}");

            // Always return success to prevent email enumeration
            return Ok(new { Success = true, Message = "If the email exists, a password reset link has been sent" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                GradeLogger.Instance.LogError("Invalid input data received for ResetPassword");
                return BadRequest(new { Success = false, Message = "Invalid input" });
            }

            var result = await _authService.ResetPasswordAsync(request);

            if (result)
            {
                GradeLogger.Instance.LogMessage($"Password reset successful");
                return Ok(new { Success = true, Message = "Password reset successful" });
            }

            GradeLogger.Instance.LogError($"Password reset failed");
            return BadRequest(new { Success = false, Message = "Invalid or expired token" });
        }
    }
}