﻿namespace GradeBookAuthAPI.Services.Interfaces
{
    public interface IEmailService
    {
        Task<string> SendPasswordResetEmailAsync(string email, string token);
    }
}
