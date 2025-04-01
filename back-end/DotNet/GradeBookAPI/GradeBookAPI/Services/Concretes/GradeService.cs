using GradeBookAPI.Data;
using GradeBookAPI.DTOs.DataDTOs;
using GradeBookAPI.Entities;
using GradeBookAPI.Helpers;
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

                // Check that the role of the user is student
                if (!await _context.Users.AnyAsync(s => s.UserId == grade.StudentId && s.Role == AuthHelper.Role.STUDENT.ToString()))
                    throw new ArgumentException("The provided user is not a student.");

                // Check that the student belongs to the class of the assignment
                var student = await _context.Users.FirstOrDefaultAsync(s => s.UserId == grade.StudentId);
                var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.AssignmentId == grade.AssignmentId);
                if (!await _context.ClassEnrollments.AnyAsync(e => e.StudentId == grade.StudentId && e.ClassId == assignment!.ClassId))
                    throw new ArgumentException("Student does not belong to the class of the assignment.");

                // The Points have to be between the assignment min and max
                if (grade.Points < assignment!.MinPoints || grade.Points > assignment.MaxPoints)
                    throw new ArgumentException($"Points have to be between the assignment minimum: {assignment.MinPoints} and maximum: {assignment.MaxPoints}");

                await _context.Grades.AddAsync(grade);
                await _context.SaveChangesAsync();
                return grade;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<GradeDto?> GetGradeByIdAsync(int gradeId)
        {
            var grade = await _context.Grades
                .Include(g => g.Assignment)
                    .ThenInclude(a => a!.Class)
                        .ThenInclude(c => c!.Course)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a!.AssignmentType)
                .Include(g => g.Student)
                    .ThenInclude(s => s!.Profile)
                .Include(g => g.Grader)
                    .ThenInclude(t => t!.Profile)
                .FirstOrDefaultAsync(g => g.GradeId == gradeId);

            if (grade == null)
                return null;

            return MapToGradeDto(grade);
        }

        public async Task<IEnumerable<GradeDto>> GetGradesAsync()
        {
            var grades = await _context.Grades
                .Include(g => g.Assignment)
                    .ThenInclude(a => a!.Class)
                        .ThenInclude(c => c!.Course)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a!.AssignmentType)
                .Include(g => g.Student)
                    .ThenInclude(s => s!.Profile)
                .Include(g => g.Grader)
                    .ThenInclude(t => t!.Profile)
                .ToListAsync();

            return grades.Select(MapToGradeDto);
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
                existingGrade.Comment = grade.Comment;
                existingGrade.GradedBy = grade.GradedBy;
                existingGrade.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

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

        public async Task<IEnumerable<GradeDto>> GetGradesForClassAsync(int classId)
        {
            // Check if the class exists
            if (!await _context.Classes.AnyAsync(c => c.ClassId == classId))
                throw new ArgumentException("Class does not exist.");

            var grades = await _context.Grades
                .Include(g => g.Assignment)
                    .ThenInclude(a => a!.Class)
                        .ThenInclude(c => c!.Course)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a!.AssignmentType)
                .Include(g => g.Student)
                    .ThenInclude(s => s!.Profile)
                .Include(g => g.Grader)
                    .ThenInclude(t => t!.Profile)
                .Where(g => g.Assignment!.ClassId == classId)
                .ToListAsync();

            return grades.Select(MapToGradeDto);
        }

        public async Task<IEnumerable<GradeDto>> GetGradesForStudentAsync(int studentId)
        {
            // Check if the student exists
            if (!await _context.Users.AnyAsync(s => s.UserId == studentId))
                throw new ArgumentException("Student does not exist.");

            var grades = await _context.Grades
                .Include(g => g.Assignment)
                    .ThenInclude(a => a!.Class)
                        .ThenInclude(c => c!.Course)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a!.AssignmentType)
                .Include(g => g.Student)
                    .ThenInclude(s => s!.Profile)
                .Include(g => g.Grader)
                    .ThenInclude(t => t!.Profile)
                .Where(g => g.StudentId == studentId)
                .ToListAsync();

            return grades.Select(MapToGradeDto);
        }

        public async Task<IEnumerable<GradeDto>> GetGradesForAClassAndStudentAsync(int classId, int studentId)
        {
            // Check if the class exists
            if (!await _context.Classes.AnyAsync(c => c.ClassId == classId))
                throw new ArgumentException("Class does not exist.");

            // Check if the student exists
            if (!await _context.Users.AnyAsync(s => s.UserId == studentId))
                throw new ArgumentException("Student does not exist.");

            var grades = await _context.Grades
                .Include(g => g.Assignment)
                    .ThenInclude(a => a!.Class)
                        .ThenInclude(c => c!.Course)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a!.AssignmentType)
                .Include(g => g.Student)
                    .ThenInclude(s => s!.Profile)
                .Include(g => g.Grader)
                    .ThenInclude(t => t!.Profile)
                .Where(g => g.Assignment!.ClassId == classId && g.StudentId == studentId)
                .ToListAsync();

            return grades.Select(MapToGradeDto);
        }

        public async Task<IEnumerable<GradeDto>> GetGradesForAssignmentAsync(int assignmentId)
        {
            // Check if the assignment exists
            if (!await _context.Assignments.AnyAsync(a => a.AssignmentId == assignmentId))
                throw new ArgumentException("Assignment does not exist.");

            var grades = await _context.Grades
                .Include(g => g.Assignment)
                    .ThenInclude(a => a!.Class)
                        .ThenInclude(c => c!.Course)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a!.AssignmentType)
                .Include(g => g.Student)
                    .ThenInclude(s => s!.Profile)
                .Include(g => g.Grader)
                    .ThenInclude(t => t!.Profile)
                .Where(g => g.AssignmentId == assignmentId)
                .ToListAsync();

            return grades.Select(MapToGradeDto);
        }

        public async Task<IEnumerable<GradeDto>> GetGradesForAssignmentAndStudentAsync(int assignmentId, int studentId)
        {
            // Check if the assignment exists
            if (!await _context.Assignments.AnyAsync(a => a.AssignmentId == assignmentId))
                throw new ArgumentException("Assignment does not exist.");

            // Check if the student exists
            if (!await _context.Users.AnyAsync(s => s.UserId == studentId))
                throw new ArgumentException("Student does not exist.");

            var grades = await _context.Grades
                .Include(g => g.Assignment)
                    .ThenInclude(a => a!.Class)
                        .ThenInclude(c => c!.Course)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a!.AssignmentType)
                .Include(g => g.Student)
                    .ThenInclude(s => s!.Profile)
                .Include(g => g.Grader)
                    .ThenInclude(t => t!.Profile)
                .Where(g => g.AssignmentId == assignmentId && g.StudentId == studentId)
                .ToListAsync();

            return grades.Select(MapToGradeDto);
        }

        private GradeDto MapToGradeDto(Grade grade)
        {
            return new GradeDto
            {
                // Grade Properties
                GradeId = grade.GradeId,
                Comment = grade.Comment,
                Points = grade.Points,
                CreatedAt = grade.CreatedAt,
                UpdatedAt = grade.UpdatedAt,

                // Grader Properties
                Grader = new GradeDto.UserDto
                {
                    UserId = grade.GradedBy,
                    FirstName = grade.Grader?.Profile?.FirstName ?? "Unknown",
                    LastName = grade.Grader?.Profile?.LastName ?? "Unknown",
                    Email = grade.Grader?.Email ?? "Unknown",
                    ProfilePicture = grade.Grader?.Profile?.ProfilePicture ?? "",
                },

                // Student Properties
                Student = new GradeDto.UserDto
                {
                    UserId = grade.StudentId,
                    FirstName = grade.Student?.Profile?.FirstName ?? "Unknown",
                    LastName = grade.Student?.Profile?.LastName ?? "Unknown",
                    Email = grade.Student?.Email ?? "Unknown",
                    ProfilePicture = grade.Student?.Profile?.ProfilePicture ?? "",
                },

                // Assignment and related entities
                Assignment = new GradeDto.AssignmentDto
                {
                    Title = grade.Assignment?.Title ?? "Unknown",
                    Description = grade.Assignment?.Description ?? "Unknown",
                    MaxPoints = grade.Assignment?.MaxPoints ?? 0,
                    MinPoints = grade.Assignment?.MinPoints ?? 0,
                    DueDate = grade.Assignment?.DueDate ?? DateTime.MinValue,
                    
                    AssignmentType = new GradeDto.AssignmentTypeDto
                    {
                        Weight = grade.Assignment?.AssignmentType?.Weight ?? 0,
                        Name = grade.Assignment?.AssignmentType?.TypeName ?? "Unknown",
                        Description = grade.Assignment?.AssignmentType?.Description ?? "Unknown",
                    },
                    
                    Class = new GradeDto.ClassDto
                    {
                        Semester = grade.Assignment?.Class?.Semester ?? "Unknown",
                        AcademicYear = grade.Assignment?.Class?.AcademicYear ?? "Unknown",
                        
                        Course = new GradeDto.CourseDto
                        {
                            Name = grade.Assignment?.Class?.Course?.CourseName ?? "Unknown",
                            Description = grade.Assignment?.Class?.Course?.Description ?? "Unknown"
                        }
                    }
                }
            };
        }
    }
}