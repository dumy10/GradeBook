using GradeBookAPI.Entities;

namespace GradeBookAPI.Services.Interfaces
{
    public interface IGradeService
    {
        Task<Grade> CreateGradeAsync(Grade grade);
        Task<Grade?> GetGradeByIdAsync(int gradeId);
        Task<IEnumerable<Grade>> GetGradesAsync();
        Task<IEnumerable<Grade>> GetGradesForClassAsync(int classId);
        Task<IEnumerable<Grade>> GetGradesForStudentAsync(int studentId);
        Task<IEnumerable<Grade>> GetGradesForAClassAndStudentAsync(int classId, int studentId);
        Task<IEnumerable<Grade>> GetGradesForAssignmentAsync(int assignmentId);
        Task<IEnumerable<Grade>> GetGradesForAssignemntAndStudentAsync(int assignmentId, int studentId);
        Task<bool> UpdateGradeAsync(Grade grade);
        Task<bool> DeleteGradeAsync(int gradeId);
    }
}
