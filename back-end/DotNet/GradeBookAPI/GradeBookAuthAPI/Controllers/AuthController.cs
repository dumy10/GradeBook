using GradeBookAuthAPI.Data;
using GradeBookAuthAPI.DTOs.AuthDTOs;
using GradeBookAuthAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GradeBookAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;

        public AuthController(IAuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

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

                return StatusCode(500, new
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
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred during login" });
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
                    return StatusCode(500, new
                    {
                        Success = false,
                        Message = "Could not connect to the database"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Database connection failed",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }
    }
}