using GradeBookAuthAPI.DTOs.AuthDTOs;

namespace GradeBookAuthAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}
