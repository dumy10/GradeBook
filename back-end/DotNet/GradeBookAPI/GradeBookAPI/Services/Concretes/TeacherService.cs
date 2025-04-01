using GradeBookAPI.Data;
using GradeBookAPI.DTOs.AuthDTOs;
using GradeBookAPI.DTOs.DataDTOs;
using GradeBookAPI.Entities;
using GradeBookAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GradeBookAPI.Services.Concretes
{
    public class TeacherService(AppDbContext context) : ITeacherService
    {
        private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        private const string STUDENT_ROLE = "STUDENT";

        public async Task<bool> AddStudentToClassAsync(int teacherId, int classId, AddStudentRequest request)
        {
            try
            {
                // Verify the class exists and belongs to the teacher
                var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.ClassId == classId && c.TeacherId == teacherId);
                if (classEntity == null)
                    return false;

                // Check if the student exists and has the role of "Student"
                var student = await _context.Users.FirstOrDefaultAsync(u => u.UserId == request.StudentId);
                if (student == null || !string.Equals(student.Role, STUDENT_ROLE, StringComparison.OrdinalIgnoreCase))
                    return false;

                // Check if the student is already enrolled
                bool alreadyEnrolled = await _context.ClassEnrollments.AnyAsync(e => e.ClassId == classId && e.StudentId == request.StudentId);
                if (alreadyEnrolled)
                    return false;
                // Create enrollment and add to db
                var enrollment = new ClassEnrollment
                {
                    ClassId = classId,
                    StudentId = request.StudentId,
                    Status = ClassEnrollmentStatus.Active.ToString(),
                    EnrollmentDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.ClassEnrollments.AddAsync(enrollment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }

        public async Task<bool> RemoveStudentFromClassAsync(int teacherId, int classId, int studentId)
        {
            try
            {
                // Verify the class exists and belongs to the teacher
                var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.ClassId == classId && c.TeacherId == teacherId);
                if (classEntity == null)
                    return false;

                // Check if the student exists and has the role of "Student"
                var student = await _context.Users.FirstOrDefaultAsync(u => u.UserId == studentId);
                if (student == null || !string.Equals(student.Role, STUDENT_ROLE, StringComparison.OrdinalIgnoreCase))
                    return false;

                // Find the enrollment to remove
                var enrollment = await _context.ClassEnrollments.FirstOrDefaultAsync(e => e.ClassId == classId && e.StudentId == studentId);
                if (enrollment == null)
                    return false;
                _context.ClassEnrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }

        public async Task<bool> AddStudentsToClassAsync(int teacherId, int classId, AddStudentsRequest request)
        {
            try
            {
                // Verify the class exists and belongs to the teacher
                var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.ClassId == classId && c.TeacherId == teacherId);
                if (classEntity == null)
                    return false;

                bool anyAdded = false;
                foreach (var studentId in request.StudentIds)
                {
                    // Check if the student is already enrolled
                    bool alreadyEnrolled = await _context.ClassEnrollments.AnyAsync(e => e.ClassId == classId && e.StudentId == studentId);

                    var student = await _context.Users.FirstOrDefaultAsync(u => u.UserId == studentId);

                    if (!alreadyEnrolled && student != null && string.Equals(student.Role, STUDENT_ROLE, StringComparison.OrdinalIgnoreCase))
                    {
                        var enrollment = new ClassEnrollment
                        {
                            ClassId = classId,
                            StudentId = studentId,
                            Status = ClassEnrollmentStatus.Active.ToString(),
                            EnrollmentDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _context.ClassEnrollments.AddAsync(enrollment);
                        anyAdded = true;
                    }
                }
                if (anyAdded)
                {
                    await _context.SaveChangesAsync();
                }
                return anyAdded;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<bool> RemoveStudentsFromClassAsync(int teacherId, int classId, RemoveStudentsRequest request)
        {
            try
            {
                // Verify the class exists and belongs to the teacher
                var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.ClassId == classId && c.TeacherId == teacherId);
                if (classEntity == null)
                    return false;

                bool anyRemoved = false;
                foreach (var studentId in request.StudentIds)
                {
                    var enrollment = await _context.ClassEnrollments.FirstOrDefaultAsync(e => e.ClassId == classId && e.StudentId == studentId);
                    var student = await _context.Users.FirstOrDefaultAsync(u => u.UserId == studentId);

                    if (enrollment != null && student != null && string.Equals(student.Role, STUDENT_ROLE, StringComparison.OrdinalIgnoreCase))
                    {
                        _context.ClassEnrollments.Remove(enrollment);
                        anyRemoved = true;
                    }
                }
                if (anyRemoved)
                {
                    await _context.SaveChangesAsync();
                }
                return anyRemoved;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<bool> CreateClassAsync(int teacherId, CreateClassRequest request)
        {
            try
            {
                // Verify that the course exists
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == request.CourseId);
                if (course == null)
                    return false;

                // Create a new class associated with the teacher and course
                var newClass = new Class
                {
                    TeacherId = teacherId,
                    CourseId = request.CourseId,
                    Semester = request.Semester,
                    AcademicYear = request.AcademicYear,
                    StartDate = DateOnly.FromDateTime(request.StartDate),
                    EndDate = DateOnly.FromDateTime(request.EndDate),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.Classes.AddAsync(newClass);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }

        public async Task<bool> CreateCourseAsync(int teacherId, CreateCourseRequest request)
        {
            // Create a new course; teacherId is not stored in Course so it is not used directly here
            var course = new Course
            {
                CourseName = request.CourseName,
                CourseCode = request.CourseCode,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                await _context.Courses.AddAsync(course);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        }

        public async Task<IEnumerable<ClassDto>> GetClassesAsync(int teacherId)
        {
            return await _context.Classes
                .Where(c => c.TeacherId == teacherId)
                .Select(c => new ClassDto
                {
                    ClassId = c.ClassId,
                    CourseId = c.CourseId,
                    Semester = c.Semester,
                    AcademicYear = c.AcademicYear,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesAsync(int teacherId)
        {
            // Retrieve distinct courses from the teacher's classes
            var courses = await _context.Classes
                .Where(c => c.TeacherId == teacherId)
                .Include(c => c.Course)
                .Select(c => c.Course!)
                .Where(course => course != null)
                .Distinct()
                .ToListAsync();

            return courses.Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                CourseCode = c.CourseCode,
                Description = c.Description
            }).ToList();
        }

        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
        {
            return await _context.Courses.Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                CourseCode = c.CourseCode,
                CourseName = c.CourseName,
                Description = c.Description
            }).ToListAsync();
        }

        public async Task<IEnumerable<StudentDto>> GetStudentsInClassAsync(int teacherId, int classId)
        {
            // Verify that the class exists and belongs to the teacher
            var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.ClassId == classId && c.TeacherId == teacherId);
            if (classEntity == null)
                return [];

            var students = await _context.ClassEnrollments
                .Where(e => e.ClassId == classId)
                .Include(e => e.Student)
                .Select(e => new StudentDto
                {
                    UserId = e.Student!.UserId,
                    FirstName = e.Student.Profile!.FirstName,
                    LastName = e.Student.Profile.LastName,
                    Email = e.Student.Email
                })
                .ToListAsync();

            return students;
        }
        public async Task<IEnumerable<UserDetailsDto>> GetUsersByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return [];

            // Convert to lowercase for case-insensitive search
            string searchTerm = name.ToLower();

            return await _context.Users
                .Include(u => u.Profile)
                .Where(u => u.Profile != null &&
                    (u.Profile.FirstName.ToLower().Contains(searchTerm) ||
                    u.Profile.LastName.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.Username.ToLower().Contains(searchTerm)))
                .Select(u => new UserDetailsDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FirstName = u.Profile!.FirstName,
                    LastName = u.Profile!.LastName,
                    Role = u.Role.ToUpperInvariant()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ClassDto>> GetClassesByNameAsync(int teacherId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return [];

            // Convert to lowercase for case-insensitive search
            string searchTerm = name.ToLower();

            return await _context.Classes
                .Include(c => c.Course)
                .Where(c => c.TeacherId == teacherId &&
                    (c.Course != null &&
                    (c.Course.CourseName.ToLower().Contains(searchTerm) ||
                    c.Course.CourseCode.ToLower().Contains(searchTerm) ||
                    c.Semester.ToLower().Contains(searchTerm) ||
                    c.AcademicYear.ToLower().Contains(searchTerm))))
                .Select(c => new ClassDto
                {
                    ClassId = c.ClassId,
                    CourseId = c.CourseId,
                    Semester = c.Semester,
                    AcademicYear = c.AcademicYear,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();
        }
    }
}
