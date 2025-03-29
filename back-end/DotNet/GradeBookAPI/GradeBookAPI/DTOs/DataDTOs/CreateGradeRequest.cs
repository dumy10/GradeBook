using System.ComponentModel.DataAnnotations;

namespace GradeBookAPI.DTOs.DataDTOs
{
    public class CreateGradeRequest
    {
        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int Points { get; set; }

        [Required]
        public required string Comment { get; set; }
    }
}
