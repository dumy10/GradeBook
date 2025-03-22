using System.ComponentModel.DataAnnotations;

namespace GradeBookAPI.DTOs.DataDTOs
{
    public class CreateGradeRequest
    {
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public int Points { get; set; }
        [Required]
        public required string Comment { get; set; } = string.Empty;
    }
}
