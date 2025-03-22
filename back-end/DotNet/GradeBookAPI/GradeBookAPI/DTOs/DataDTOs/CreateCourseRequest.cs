namespace GradeBookAPI.DTOs.DataDTOs
{
    public class CreateCourseRequest
    {
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
