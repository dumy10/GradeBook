namespace GradeBookAPI.DTOs.DataDTOs
{
    public class StudentDto
    {
        public int StudentId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
    }
}
