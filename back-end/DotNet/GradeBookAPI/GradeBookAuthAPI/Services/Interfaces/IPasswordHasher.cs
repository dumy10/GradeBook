﻿namespace GradeBookAuthAPI.Services.Interfaces
{
    public interface IPasswordHasher
    {
        byte[] GenerateSalt();
        string HashPassword(string password, byte[] salt);
        bool VerifyPassword(string password, string hash, byte[] salt);
    }
}
