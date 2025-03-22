using GradeBookAPI.Data;
using GradeBookAPI.DTOs.DataDTOs;
using GradeBookAPI.Entities;
using GradeBookAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GradeBookAPI.Services.Concretes
{
    public class AssignmentService(AppDbContext context) : IAssignmentService
    {
        private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<Assignment> CreateAssignmentAsync(CreateAssignmentRequest request)
        {
            try
            {
                var newType = new AssignmentType
                {
                    TypeName = request.TypeName,
                    Weight = request.Weight,
                    Description = $"Type for assignment: {request.Title}. The description of the assignment: {request.Description}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.AssignmentTypes.Add(newType);
                await _context.SaveChangesAsync();

                var assignment = new Assignment
                {
                    ClassId = request.ClassId,
                    TypeId = newType.TypeId,
                    Title = request.Title,
                    Description = request.Description,
                    MaxPoints = request.MaxPoints,
                    MinPoints = request.MinPoints,
                    DueDate = request.DueDate,
                    IsPublished = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Assignments.Add(assignment);
                await _context.SaveChangesAsync();

                return assignment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteAssignmentAsync(int assignmentId)
        {
            try
            {
                var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

                if (assignment == null)
                    return false;

                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Assignment>> GetAllAssignmentsAsync()
        {
            return await _context.Assignments.Include(a => a.AssignmentType).ToListAsync();
        }

        public async Task<IEnumerable<Assignment>> GetAssignmentsForClassAsync(int classId)
        {
            // Check if the class exists
            if (!await _context.Classes.AnyAsync(c => c.ClassId == classId))
                throw new ArgumentException("Class does not exist.");

            return await _context.Assignments.Include(a => a.AssignmentType).Where(a => a.ClassId == classId).ToListAsync();
        }

        public async Task<Assignment?> UpdateAssignmentAsync(int assignmentId, CreateAssignmentRequest request)
        {
            try
            {
                var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

                if (assignment == null)
                    return null;
                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();
                return assignment;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}