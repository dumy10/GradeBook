using GradeBookAPI.DTOs.DataDTOs;
using GradeBookAPI.Entities;

namespace GradeBookAPI.Services.Interfaces
{
    public interface IAssignmentService
    {
        Task<Assignment> CreateAssignmentAsync(CreateAssignmentRequest request);
        Task<Assignment?> UpdateAssignmentAsync(int assignmentId, CreateAssignmentRequest request);
        Task<bool> DeleteAssignmentAsync(int assignmentId);
        Task<IEnumerable<Assignment>> GetAssignmentsForClassAsync(int classId);
        Task<IEnumerable<Assignment>> GetAllAssignmentsAsync();
    }
}
