using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GradeBookAPI.Entities
{
    public class Assignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        [ForeignKey("Class")]
        public int ClassId { get; set; }

        [Required]
        [ForeignKey("AssignmentType")]
        public int TypeId { get; set; }

        [Required]
        [StringLength(100)]
        public required string Title { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public int MaxPoints { get; set; }

        [Required]
        public int MinPoints { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Class? Class { get; set; }
        public virtual AssignmentType? AssignmentType { get; set; }

    }
}
