using System.ComponentModel.DataAnnotations;

namespace GradeBookAPI.Entities
{
    public class AssignmentType
    {
        [Key]
        public int TypeId { get; set; }

        [Required]
        [StringLength(50)]
        public required string TypeName { get; set; }

        [Required]
        public int Weight { get; set; }

        [Required]
        public required string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


    }
}
