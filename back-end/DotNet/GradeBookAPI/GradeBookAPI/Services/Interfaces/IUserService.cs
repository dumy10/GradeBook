using GradeBookAPI.DTOs.AuthDTOs;

namespace GradeBookAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task<bool> UpdateUserProfileAsync(int userId, UpdateProfileRequest request);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<UserDetailsDto?> GetUserDetailsAsync(int userId);
        Task<IEnumerable<UserDetailsDto>> GetAllUsersDetailsAsync();
    }
}
