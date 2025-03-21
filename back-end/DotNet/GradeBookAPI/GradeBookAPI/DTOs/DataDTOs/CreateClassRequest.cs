namespace GradeBookAPI.DTOs.DataDTOs
{
    public class CreateClassRequest
    {
        public int CourseId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Semester { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
