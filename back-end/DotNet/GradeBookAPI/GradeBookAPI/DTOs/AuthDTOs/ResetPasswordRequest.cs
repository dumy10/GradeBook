using System.ComponentModel.DataAnnotations;

namespace GradeBookAPI.DTOs.AuthDTOs
{
    public class ResetPasswordRequest
    {
        [Required]
        public required string Token { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public required string NewPassword { get; set; }
        [Required]
        [Compare("NewPassword")]
        public required string ConfirmPassword { get; set; }
    }
}
