namespace GradeBookAPI.DTOs.AuthDTOs
{
    public class AuthResponse
    {
        public int UserId { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public bool Success { get; set; }
    }
}
