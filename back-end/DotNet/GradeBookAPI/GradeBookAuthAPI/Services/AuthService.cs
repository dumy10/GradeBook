using GradeBookAuthAPI.Data;
using GradeBookAuthAPI.DTOs.AuthDTOs;
using GradeBookAuthAPI.Entities;
using GradeBookAuthAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GradeBookAuthAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;

        public AuthService(AppDbContext context, IConfiguration configuration, IPasswordHasher passwordHasher, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email already registered"
                };
            }

            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Username already taken"
                };
            }

            // Validate role
            if (string.Compare(request.Role, "Teacher", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(request.Role, "Student", StringComparison.OrdinalIgnoreCase) != 0)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid role. Must be 'Teacher' or 'Student'"
                };
            }

            if (!IsValidEmail(request.Email))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email format"
                };
            }

            if (!IsValidName(request.FirstName))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "First name cannot be empty or contain whitespace"
                };
            }

            if (!IsValidName(request.LastName))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Last name cannot be empty or contain whitespace"
                };
            }

            // Generate salt and hash password
            var salt = _passwordHasher.GenerateSalt();
            var passwordHash = _passwordHasher.HashPassword(request.Password, salt);

            // Create new user
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                Salt = Convert.ToBase64String(salt),
                Role = request.Role.ToUpperInvariant(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create user profile
            var profile = new UserProfile
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add user and profile to database
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.Users.AddAsync(user);
                    await _context.SaveChangesAsync();

                    profile.UserId = user.UserId;
                    await _context.UserProfiles.AddAsync(profile);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Registration successful",
                Token = token,
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // Find user by username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            // Check if user exists
            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            // Verify password
            var salt = Convert.FromBase64String(user.Salt);
            bool isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash, salt);

            if (!isPasswordValid)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            // Update last login time
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // If user doesn't exist, still return true to prevent email enumeration
            if (user == null)
                return true;

            // Generate a secure random token
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var token = Convert.ToBase64String(randomBytes)
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");

            // Create password reset record
            var passwordReset = new PasswordReset
            {
                UserId = user.UserId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow
            };

            // Save to database
            await _context.PasswordResets.AddAsync(passwordReset);
            await _context.SaveChangesAsync();

            // Generate reset link (in production, this would send an email)
            string resetLink = await _emailService.SendPasswordResetEmailAsync(email, token);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            // Find valid reset token
            var passwordReset = await _context.PasswordResets
                .Include(r => r.User)
                .FirstOrDefaultAsync(r =>
                    r.Token == request.Token &&
                    r.UsedAt == null &&
                    r.ExpiresAt > DateTime.UtcNow);

            if (passwordReset == null)
                return false;

            var user = passwordReset.User;

            // Update password
            var salt = _passwordHasher.GenerateSalt();
            var passwordHash = _passwordHasher.HashPassword(request.NewPassword, salt);

            user.PasswordHash = passwordHash;
            user.Salt = Convert.ToBase64String(salt);
            user.UpdatedAt = DateTime.UtcNow;

            // Mark token as used
            passwordReset.UsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Use .NET's built-in email validation
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && !name.Contains(" ");
        }


        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET"));

            var claims = new List<Claim>
            {
                 new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                 new Claim(ClaimTypes.Email, user.Email),
                 new Claim(ClaimTypes.Name, user.Username),
                 new Claim(ClaimTypes.Role, user.Role)
             };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(double.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_HOURS"))),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
                Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
