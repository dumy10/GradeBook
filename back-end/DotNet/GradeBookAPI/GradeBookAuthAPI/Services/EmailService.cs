using GradeBookAuthAPI.Services.Interfaces;

namespace GradeBookAuthAPI.Services
{
    public class EmailService : IEmailService
    {
        public Task<string> SendPasswordResetEmailAsync(string email, string token)
        {
            string resetLink = $"http://localhost:4200/reset-password?token={token}";
            Console.WriteLine($"[DEV MODE] Password reset for {email}: {resetLink}");
            return Task.FromResult(resetLink);
        }
    }
}
