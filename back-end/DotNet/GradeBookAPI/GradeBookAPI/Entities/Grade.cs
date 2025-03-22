using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GradeBookAPI.Entities
{
    public class Grade
    {
        [Key]
        public int GradeId { get; set; }

        [Required]
        [ForeignKey("Assignment")]
        public int AssignmentId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int StudentId { get; set; }

        [Required]
        public int Points { get; set; }

        [Required]
        public required string Comment { get; set; }

        [Required]
        [ForeignKey("User")]
        public int GradedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Assignment? Assignment { get; set; }
        public virtual UserProfile? Student { get; set; }
        public virtual UserProfile? Grader { get; set; }
    }
}
