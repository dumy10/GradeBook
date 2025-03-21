using GradeBookAPI.Data;
using GradeBookAPI.DTOs.AuthDTOs;
using GradeBookAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                    return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
                }

                var result = await _authService.RegisterAsync(request);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                // Include more detailed error information for debugging
                Console.WriteLine($"Registration error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "An error occurred during registration",
                    // Only include these details during development
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
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
                    return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
                }

                var result = await _authService.LoginAsync(request);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception)
            {
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
                    return StatusCode(StatusCodes.Status500InternalServerError, new
                    {
                        Success = false,
                        Message = "Could not connect to the database"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "Database connection failed",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }

        // AuthController.cs
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Success = false, Message = "Invalid email format" });
            }

            await _authService.ForgotPasswordAsync(request.Email);

            // Always return success to prevent email enumeration
            return Ok(new { Success = true, Message = "If the email exists, a password reset link has been sent" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Success = false, Message = "Invalid input" });
            }

            var result = await _authService.ResetPasswordAsync(request);

            if (result)
            {
                return Ok(new { Success = true, Message = "Password reset successful" });
            }

            return BadRequest(new { Success = false, Message = "Invalid or expired token" });
        }

    }
}