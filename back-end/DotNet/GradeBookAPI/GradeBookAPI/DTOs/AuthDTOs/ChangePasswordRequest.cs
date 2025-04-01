using System.ComponentModel.DataAnnotations;

namespace GradeBookAPI.DTOs.AuthDTOs
{
    public class ChangePasswordRequest
    {
        [Required]
        public required string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public required string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword")]
        public required string ConfirmPassword { get; set; } = string.Empty;
    }
}
