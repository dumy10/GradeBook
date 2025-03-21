using System.ComponentModel.DataAnnotations;

namespace GradeBookAPI.DTOs.AuthDTOs
{
    public class LoginRequest
    {
        [Required]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}