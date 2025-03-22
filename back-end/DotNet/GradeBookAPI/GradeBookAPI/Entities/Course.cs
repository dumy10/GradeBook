using System.ComponentModel.DataAnnotations;

namespace GradeBookAPI.Entities
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        [StringLength(100)]
        public required string CourseName { get; set; }

        [Required]
        [StringLength(20)]
        public required string CourseCode { get; set; }
        
        [Required]
        public required string Description { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Class>? Classes { get; set; }
    }
}
