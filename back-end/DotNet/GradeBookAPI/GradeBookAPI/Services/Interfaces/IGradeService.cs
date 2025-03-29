using GradeBookAPI.DTOs.DataDTOs;
using GradeBookAPI.Entities;

namespace GradeBookAPI.Services.Interfaces
{
    public interface IGradeService
    {
        Task<Grade> CreateGradeAsync(Grade grade);
        Task<GradeDto?> GetGradeByIdAsync(int gradeId);
        Task<IEnumerable<GradeDto>> GetGradesAsync();
        Task<bool> UpdateGradeAsync(Grade grade);
        Task<bool> DeleteGradeAsync(int gradeId);
        Task<IEnumerable<GradeDto>> GetGradesForClassAsync(int classId);
        Task<IEnumerable<GradeDto>> GetGradesForStudentAsync(int studentId);
        Task<IEnumerable<GradeDto>> GetGradesForAClassAndStudentAsync(int classId, int studentId);
        Task<IEnumerable<GradeDto>> GetGradesForAssignmentAsync(int assignmentId);
        Task<IEnumerable<GradeDto>> GetGradesForAssignmentAndStudentAsync(int assignmentId, int studentId);
    }
}
