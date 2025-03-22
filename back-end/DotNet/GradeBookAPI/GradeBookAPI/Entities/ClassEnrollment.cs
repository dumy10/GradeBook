using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GradeBookAPI.Entities
{
    public class ClassEnrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        [ForeignKey("Class")]
        public int ClassId { get; set; }

        [ForeignKey("User")]
        public int StudentId { get; set; }

        public DateTime EnrollmentDate { get; set; } 

        [StringLength(10)]
        public required string Status { get; set; }

        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; } 

        public virtual Class? Class { get; set; }
        public virtual User? Student { get; set; }
    }
}