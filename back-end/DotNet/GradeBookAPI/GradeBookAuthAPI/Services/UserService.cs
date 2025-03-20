using GradeBookAuthAPI.Data;
using GradeBookAuthAPI.DTOs.AuthDTOs;
using GradeBookAuthAPI.Entities;
using GradeBookAuthAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GradeBookAuthAPI.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(AppDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
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
            if (user.Profile == null)
            {
                user.Profile = new UserProfile
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
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
    }
}