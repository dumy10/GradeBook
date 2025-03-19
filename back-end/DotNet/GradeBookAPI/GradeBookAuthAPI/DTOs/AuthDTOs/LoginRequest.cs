using System.ComponentModel.DataAnnotations;

namespace GradeBookAuthAPI.DTOs.AuthDTOs
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}