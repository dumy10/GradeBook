using GradeBookAuthAPI.DTOs.AuthDTOs;

namespace GradeBookAuthAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task<bool> UpdateUserProfileAsync(int userId, UpdateProfileRequest request);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    }
}
