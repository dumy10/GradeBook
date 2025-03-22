namespace GradeBookAPI.DTOs.DataDTOs
{
    public class CreateAssignmentRequest
    {
        public int ClassId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int MaxPoints { get; set; }
        public int MinPoints { get; set; }
        public DateTime DueDate { get; set; }

        // AssignmentType properties
        public string TypeName { get; set; } = null!;
        public int Weight { get; set; }
    }
}