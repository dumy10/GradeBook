namespace GradeBookAPI.DTOs.DataDTOs
{
    public class GradeDto
    {
        #region Grade Properties
        public int GradeId { get; set; }
        public required string Comment { get; set; }
        public int Points { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        #endregion

        #region Related Entities
        public required UserDto Grader { get; set; }
        public required UserDto Student { get; set; }
        public required AssignmentDto Assignment { get; set; }
        #endregion

        public class UserDto
        {
            public int UserId { get; set; }
            public required string FirstName { get; set; }
            public required string LastName { get; set; }
            public required string Email { get; set; }
            public required string ProfilePicture { get; set; }
        }

        public class AssignmentDto
        {
            public required string Title { get; set; }
            public required string Description { get; set; }
            public int MaxPoints { get; set; }
            public int MinPoints { get; set; }
            public DateTime DueDate { get; set; }
            public required AssignmentTypeDto AssignmentType { get; set; }
            public required ClassDto Class { get; set; }
        }

        public class AssignmentTypeDto
        {
            public int Weight { get; set; }
            public required string Name { get; set; }
            public required string Description { get; set; }
        }

        public class ClassDto
        {
            public required string Semester { get; set; }
            public required string AcademicYear { get; set; }
            public required CourseDto Course { get; set; }
        }

        public class CourseDto
        {
            public required string Name { get; set; }
            public required string Description { get; set; }
        }
    }
}
