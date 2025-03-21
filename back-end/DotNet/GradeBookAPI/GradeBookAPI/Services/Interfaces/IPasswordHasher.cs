namespace GradeBookAPI.Services.Interfaces
{
    public interface IPasswordHasher
    {
        byte[] GenerateSalt();
        bool VerifyPassword(string password, string hash, byte[] salt);
        string HashPassword(string password, byte[] salt);
    }
}
