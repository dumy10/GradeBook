using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GradeBookAPI.Entities
{
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public required string Action { get; set; }

        [Required]
        [StringLength(50)]
        public required string EntityType { get; set; }

        [Required]
        public int EntityId { get; set; }

        [Required]
        public required string Details { get; set; }

        [Required]
        [StringLength(45)]
        public required string IpAddress { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public virtual User? User { get; set; }
    }
}
