using GradeBookAPI.Data;
using GradeBookAPI.Entities;
using GradeBookAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GradeBookAPI.Services.Concretes
{
    public class GradeService(AppDbContext context) : IGradeService
    {
        private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<Grade> CreateGradeAsync(Grade grade)
        {
            try
            {
                // Check that the assignment exists
                if (!await _context.Assignments.AnyAsync(a => a.AssignmentId == grade.AssignmentId))
                    throw new ArgumentException("Assignment does not exist.");

                // Check that the student exists
                if (!await _context.Users.AnyAsync(s => s.UserId == grade.StudentId))
                    throw new ArgumentException("Student does not exist.");

                // Check that the student belongs to the class of the assignment
                var student = await _context.Users.FirstOrDefaultAsync(s => s.UserId == grade.StudentId);
                var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.AssignmentId == grade.AssignmentId);
                if (!await _context.ClassEnrollments.AnyAsync(e => e.StudentId == grade.StudentId && e.ClassId == assignment!.ClassId))
                    throw new ArgumentException("Student does not belong to the class of the assignment.");

                // The Points have to be between the assignment min and max
                if (grade.Points < assignment!.MinPoints || grade.Points > assignment.MaxPoints)
                    throw new ArgumentException("Points have to be between the assignment min and max.");

                await _context.Grades.AddAsync(grade);
                await _context.SaveChangesAsync();
                return grade;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Grade?> GetGradeByIdAsync(int gradeId)
        {
            return await _context.Grades
                .Include(g => g.Assignment)
                .Include(g => g.Student)
                .Include(g => g.Grader)
                .FirstOrDefaultAsync(g => g.GradeId == gradeId);
        }

        public async Task<IEnumerable<Grade>> GetGradesAsync()
        {
            return await _context.Grades
                .Include(g => g.Assignment)
                .Include(g => g.Student)
                .Include(g => g.Grader)
                .ToListAsync();
        }

        public async Task<bool> UpdateGradeAsync(Grade grade)
        {
            try
            {
                // Check that the assignment exists
                if (!await _context.Assignments.AnyAsync(a => a.AssignmentId == grade.AssignmentId))
                    throw new ArgumentException("Assignment does not exist.");

                // Check that the student exists
                if (!await _context.Users.AnyAsync(s => s.UserId == grade.StudentId))
                    throw new ArgumentException("Student does not exist.");

                // Check that the student belongs to the class of the assignment
                var student = await _context.Users.FirstOrDefaultAsync(s => s.UserId == grade.StudentId);
                var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.AssignmentId == grade.AssignmentId);
                if (!await _context.ClassEnrollments.AnyAsync(e => e.StudentId == grade.StudentId && e.ClassId == assignment!.ClassId))
                    throw new ArgumentException("Student does not belong to the class of the assignment.", nameof(grade));

                var existingGrade = await _context.Grades.FirstOrDefaultAsync(g => g.GradeId == grade.GradeId);
                if (existingGrade == null)
                    return false;
                // Update only allowed fields
                existingGrade.Points = grade.Points;
                existingGrade.GradedBy = grade.GradedBy;
                existingGrade.UpdatedAt = DateTime.UtcNow;
                _context.Grades.Update(existingGrade);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteGradeAsync(int gradeId)
        {
            try
            {
                var grade = await _context.Grades.FirstOrDefaultAsync(g => g.GradeId == gradeId);
                if (grade == null)
                    return false;
                _context.Grades.Remove(grade);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Grade>> GetGradesForClassAsync(int classId)
        {
            // Check if the class exists
            if (!await _context.Classes.AnyAsync(c => c.ClassId == classId))
                throw new ArgumentException("Class does not exist.");

            return await _context.Grades
                .Include(g => g.Assignment)
                .Include(g => g.Student)
                .Include(g => g.Grader)
                .Where(g => g.Assignment!.ClassId == classId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Grade>> GetGradesForStudentAsync(int studentId)
        {
            // Check if the student exists
            if (!await _context.Users.AnyAsync(s => s.UserId == studentId))
                throw new ArgumentException("Student does not exist.");

            return await _context.Grades
                .Include(g => g.Assignment)
                .Include(g => g.Student)
                .Include(g => g.Grader)
                .Where(g => g.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Grade>> GetGradesForAClassAndStudentAsync(int classId, int studentId)
        {
            // Check if the class exists
            if (!await _context.Classes.AnyAsync(c => c.ClassId == classId))
                throw new ArgumentException("Class does not exist.");

            // Check if the student exists
            if (!await _context.Users.AnyAsync(s => s.UserId == studentId))
                throw new ArgumentException("Student does not exist.");

            return await _context.Grades
                .Include(g => g.Assignment)
                .Include(g => g.Student)
                .Include(g => g.Grader)
                .Where(g => g.Assignment!.ClassId == classId && g.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Grade>> GetGradesForAssignmentAsync(int assignmentId)
        {
            // Check if the assignment exists
            if (!await _context.Assignments.AnyAsync(a => a.AssignmentId == assignmentId))
                throw new ArgumentException("Assignment does not exist.");

            return await _context.Grades
                .Include(g => g.Assignment)
                .Include(g => g.Student)
                .Include(g => g.Grader)
                .Where(g => g.AssignmentId == assignmentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Grade>> GetGradesForAssignemntAndStudentAsync(int assignmentId, int studentId)
        {
            // Check if the assignment exists
            if (!await _context.Assignments.AnyAsync(a => a.AssignmentId == assignmentId))
                throw new ArgumentException("Assignment does not exist.");

            // Check if the student exists
            if (!await _context.Users.AnyAsync(s => s.UserId == studentId))
                throw new ArgumentException("Student does not exist.");

            return await _context.Grades
                .Include(g => g.Assignment)
                .Include(g => g.Student)
                .Include(g => g.Grader)
                .Where(g => g.AssignmentId == assignmentId && g.StudentId == studentId)
                .ToListAsync();
        }
    }
}