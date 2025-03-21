namespace GradeBookAPI.DTOs.DataDTOs
{
    public class ClassDto
    {
        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public string Semester { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
