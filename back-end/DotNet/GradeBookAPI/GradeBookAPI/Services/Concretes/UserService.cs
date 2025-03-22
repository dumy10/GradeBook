using GradeBookAPI.Data;
using GradeBookAPI.DTOs.AuthDTOs;
using GradeBookAPI.Entities;
using GradeBookAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace GradeBookAPI.Services.Concretes
{
    public partial class UserService(AppDbContext context, IPasswordHasher passwordHasher) : IUserService
    {
        private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IPasswordHasher _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return null;
            }

            return new UserProfileDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.Profile?.FirstName,
                LastName = user.Profile?.LastName,
                Phone = user.Profile?.Phone,
                Address = user.Profile?.Address,
                Role = user.Role
            };
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, UpdateProfileRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return false;
            }

            // Create profile if it doesn't exist
            user.Profile ??= new UserProfile
            {
                UserId = userId,
                FirstName = request.FirstName ?? string.Empty,
                LastName = request.LastName ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            // Validate phone number
            if (!IsValidPhoneNumber(request.Phone))
            {
                return false;
            }

            // Update profile fields
            user.Profile.FirstName = request.FirstName ?? user.Profile.FirstName;
            user.Profile.LastName = request.LastName ?? user.Profile.LastName;
            user.Profile.Phone = request.Phone ?? user.Profile.Phone;
            user.Profile.Address = request.Address ?? user.Profile.Address;
            user.Profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return false;
            }

            // Verify current password
            var salt = Convert.FromBase64String(user.Salt);
            bool isPasswordValid = _passwordHasher.VerifyPassword(
                request.CurrentPassword,
                user.PasswordHash,
                salt
            );

            if (!isPasswordValid)
            {
                return false;
            }

            // Update password
            var newSalt = _passwordHasher.GenerateSalt();
            var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword, newSalt);

            user.PasswordHash = newPasswordHash;
            user.Salt = Convert.ToBase64String(newSalt);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserDetailsDto?> GetUserDetailsAsync(int userId)
        {
            var users = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (users == null)
            {
                return null;
            }

            return new UserDetailsDto
            {
                UserId = users.UserId,
                Email = users.Email,
                FirstName = users.Profile!.FirstName,
                LastName = users.Profile!.LastName,
                Role = users.Role.ToUpperInvariant()
            };
        }

        public async Task<IEnumerable<UserDetailsDto>> GetAllUsersDetailsAsync()
        {
            return await _context.Users
                .Include(u => u.Profile)
                .Select(u => new UserDetailsDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FirstName = u.Profile!.FirstName,
                    LastName = u.Profile!.LastName,
                    Role = u.Role.ToUpperInvariant()
                })
                .ToListAsync();
        }

        private static bool IsValidPhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) // The Phone number is not required so we can return true if it is empty or null
            {
                return true;
            }

            // Remove any whitespace characters
            _ = WhiteSpaceRegexPattern().Replace(phoneNumber, "");

            var phoneNumberPattern = @"^\+\d{1,3}\d{4,14}$"; // The number must start with a '+' followed by 1-3 digits and then 4-14 digits

            return Regex.IsMatch(phoneNumber, phoneNumberPattern);
        }

        [GeneratedRegex(@"\s+")] // This attribute is used to generate the regex pattern for the WhiteSpaceRegexPattern method. This is generated at compile time.
        private static partial Regex WhiteSpaceRegexPattern();

    }
}