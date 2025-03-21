using GradeBookAPI.DTOs.DataDTOs;

namespace GradeBookAPI.Services.Interfaces
{
    public interface ITeacherService
    {
        Task<IEnumerable<ClassDto>> GetClassesAsync(int teacherId);
        Task<IEnumerable<CourseDto>> GetCoursesAsync(int teacherId);
        Task<IEnumerable<CourseDto>> GetAllCoursesAsync();

        Task<bool> CreateClassAsync(int teacherId, CreateClassRequest request);
        Task<bool> CreateCourseAsync(int teacherId, CreateCourseRequest request);

        Task<bool> AddStudentToClassAsync(int teacherId, int classId, AddStudentRequest request);
        Task<bool> RemoveStudentFromClassAsync(int teacherId, int classId, int studentId);

        Task<bool> AddStudentsToClassAsync(int teacherId, int classId, AddStudentsRequest request);
        Task<bool> RemoveStudentsFromClassAsync(int teacherId, int classId, RemoveStudentsRequest request);

        Task<IEnumerable<StudentDto>> GetStudentsInClassAsync(int teacherId, int classId);
    }
}
