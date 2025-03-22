namespace GradeBookAPI.DTOs.DataDTOs
{
    public class UpdateGradeRequest
    {
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public int Points { get; set; }
        public required string Comment { get; set; } = string.Empty;
    }
}
