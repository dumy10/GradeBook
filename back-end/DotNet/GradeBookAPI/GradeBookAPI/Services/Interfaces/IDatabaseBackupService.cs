namespace GradeBookAPI.Services.Interfaces
{
    public interface IDatabaseBackupService
    {
        Task<bool> CreateBackupAsync();
        Task<bool> RestoreBackupAsync(string backupFilePath);
        string[] GetAvailableBackups();
    }
}