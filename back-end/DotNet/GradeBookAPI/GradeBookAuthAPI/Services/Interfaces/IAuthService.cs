using System.Threading.Tasks;
using GradeBookAuthAPI.DTOs.AuthDTOs;

namespace GradeBookAuthAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        // Add other authentication methods as needed
    }
}
