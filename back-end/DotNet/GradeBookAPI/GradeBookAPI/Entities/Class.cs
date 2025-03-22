using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GradeBookAPI.Entities
{
    public class Class
    {
        [Key]
        public int ClassId { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }

        [ForeignKey("User")]
        public int TeacherId { get; set; }

        [Required]
        [StringLength(20)]
        public required string Semester { get; set; }

        [Required]
        [StringLength(10)]
        public required string AcademicYear { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly EndDate { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Course? Course { get; set; }
        public virtual User? Teacher { get; set; }
        public virtual ICollection<ClassEnrollment>? Enrollments { get; set; }
        public virtual ICollection<Assignment>? Assignments { get; set; }
    }
}
