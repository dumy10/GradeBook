﻿using GradeBookAPI.Services.Interfaces;
using System.Security.Cryptography;

namespace GradeBookAPI.Services.Concretes
{
    public class PasswordHasher : IPasswordHasher
    {
        public byte[] GenerateSalt()
        {
            var salt = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public bool VerifyPassword(string password, string storedHash, byte[] salt)
        {
            string computedHash = HashPassword(password, salt);
            return storedHash.Equals(computedHash);
        }

        public string HashPassword(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);
            return Convert.ToBase64String(hash);
        }

    }
}