using GradeBookAPI.DTOs.DataDTOs;
using GradeBookAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using GradeLogger = GradeBookAPI.Logger.Logger;

namespace GradeBookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // We need authentication for all endpoints in this controller
    public class TeacherController(ITeacherService teacherService) : ControllerBase
    {
        private readonly ITeacherService _teacherService = teacherService;

        private const string TEACHER_ROLE = "TEACHER";

        // POST api/Teacher/classes/{classId}/students
        [HttpPost("classes/{classId}/students")]
        public async Task<IActionResult> AddStudent(int classId, [FromBody] AddStudentRequest request)
        {
            if (!ModelState.IsValid)
            {
                GradeLogger.Instance.LogError("Invalid input data received for AddStudent");
                return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
            }

            if (!ValidateTeacherToken(out int teacherId))
            {
                GradeLogger.Instance.LogError($"Invalid or expired token received for AddStudent. Teacher id: {teacherId}");
                return Unauthorized("Invalid or expired token.");
            }

            var result = await _teacherService.AddStudentToClassAsync(teacherId, classId, request);
            if (!result)
            {
                GradeLogger.Instance.LogError($"Unable to add student. Student:{request.StudentId}");
                return BadRequest("Unable to add student.");
            }

            GradeLogger.Instance.LogMessage($"Student added successfully. Student:{request.StudentId}. Class:{classId}");
            return Ok("Student added successfully.");
        }

        // DELETE api/Teacher/classes/{classId}/students/{studentId}
        [HttpDelete("classes/{classId}/students/{studentId}")]
        public async Task<IActionResult> RemoveStudent(int classId, int studentId)
        {
            if (!ValidateTeacherToken(out int teacherId))
            {
                GradeLogger.Instance.LogError($"Invalid or expired token received for RemoveStudent. Teacher id: {teacherId}");
                return Unauthorized("Invalid or expired token.");
            }

            var result = await _teacherService.RemoveStudentFromClassAsync(teacherId, classId, studentId);
            if (!result)
            {
                GradeLogger.Instance.LogError($"Unable to remove student. Student:{studentId}. Class:{classId}");
                return BadRequest("Unable to remove student.");
            }

            GradeLogger.Instance.LogMessage($"Student removed successfully. Student:{studentId}. Class:{classId}");
            return Ok("Student removed successfully.");
        }

        // POST api/Teacher/classes/{classId}/students/batch
        [HttpPost("classes/{classId}/students/batch")]
        public async Task<IActionResult> AddStudents(int classId, [FromBody] AddStudentsRequest request)
        {
            if (!ModelState.IsValid)
            {
                GradeLogger.Instance.LogError("Invalid input data received for AddStudents");
                return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
            }

            if (!ValidateTeacherToken(out int teacherId))
            {
                GradeLogger.Instance.LogError($"Invalid or expired token received for AddStudents. Teacher id: {teacherId}");
                return Unauthorized("Invalid or expired token.");
            }

            var result = await _teacherService.AddStudentsToClassAsync(teacherId, classId, request);
            if (!result)
            {
                GradeLogger.Instance.LogError("Unable to add some or all students.");
                return BadRequest("Unable to add some or all students.");
            }

            GradeLogger.Instance.LogMessage("Students added successfully.");
            return Ok("Students added successfully.");
        }

        // DELETE api/Teacher/classes/{classId}/students/batch
        [HttpDelete("classes/{classId}/students/batch")]
        public async Task<IActionResult> RemoveStudents(int classId, [FromBody] RemoveStudentsRequest request)
        {
            if (!ModelState.IsValid)
            {
                GradeLogger.Instance.LogError("Invalid input data received for RemoveStudents");
                return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
            }

            if (!ValidateTeacherToken(out int teacherId))
            {
                GradeLogger.Instance.LogError($"Invalid or expired token received for RemoveStudents. Teacher id: {teacherId}");
                return Unauthorized("Invalid or expired token.");
            }

            var result = await _teacherService.RemoveStudentsFromClassAsync(teacherId, classId, request);
            if (!result)
            {
                GradeLogger.Instance.LogError("Unable to remove some or all students.");
                return BadRequest("Unable to remove some or all students.");
            }

            GradeLogger.Instance.LogMessage("Students removed successfully.");
            return Ok("Students removed successfully.");
        }

        // POST api/Teacher/courses
        [HttpPost("courses")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
        {
            if (!ModelState.IsValid)
            {
                GradeLogger.Instance.LogError("Invalid input data received for CreateCourse");
                return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
            }

            if (!ValidateTeacherToken(out int teacherId))
            {
                GradeLogger.Instance.LogError($"Invalid or expired token received for CreateCourse. Teacher id: {teacherId}");
                return Unauthorized("Invalid or expired token.");
            }
            try
            {
                var result = await _teacherService.CreateCourseAsync(teacherId, request);

                if (!result)
                {
                    GradeLogger.Instance.LogError("Course creation failed.");
                    return BadRequest("Course creation failed.");
                }
            }
            catch (Exception ex)
            {
                GradeLogger.Instance.LogError($"Course creation failed. Exception: {ex.Message}");

                return BadRequest("Course creation failed.");
            }


            GradeLogger.Instance.LogMessage("Course created successfully.");
            return Ok("Course created successfully.");
        }

        // POST api/Teacher/classes
        [HttpPost("classes")]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassRequest request)
        {
            if (!ModelState.IsValid)
            {
                GradeLogger.Instance.LogError("Invalid input data received for CreateClass");
                return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
            }
            if (!ValidateTeacherToken(out int teacherId))
            {
                GradeLogger.Instance.LogError($"Invalid or expired token received for CreateClass. Teacher id: {teacherId}");
                return Unauthorized("Invalid or expired token.");
            }

            try
            {
                var result = await _teacherService.CreateClassAsync(teacherId, request);

                if (!result)
                {
                    GradeLogger.Instance.LogError("Class creation failed. Verify that the course exists.");
                    return BadRequest("Class creation failed. Verify that the course exists.");
                }
            }
            catch (Exception ex)
            {
                GradeLogger.Instance.LogError($"Class creation failed. Exception: {ex.Message}");
                return BadRequest("Class creation failed.");
            }


            GradeLogger.Instance.LogMessage("Class created successfully.");
            return Ok("Class created successfully.");
        }

        // GET api/Teacher/classes
        [HttpGet("classes")]
        public async Task<IActionResult> GetClasses()
        {
            if (!ValidateTeacherToken(out int teacherId))
            {
                GradeLogger.Instance.LogError($"Invalid or expired token received for GetClasses. Teacher id: {teacherId}");
                return Unauthorized("Invalid or expired token.");
            }

            var classes = await _teacherService.GetClassesAsync(teacherId);

            GradeLogger.Instance.LogMessage("Classes retrieved successfully.");
            return Ok(classes);
        }

        // GET api/Teacher/courses/assigned
        [HttpGet("courses/assigned")]
        public async Task<IActionResult> GetAssignedCourses()
        {
            if (!ValidateTeacherToken(out int teacherId))
            {
                GradeLogger.Instance.LogError($"Invalid or expired token received for GetCourses. Teacher id: {teacherId}");
                return Unauthorized("Invalid or expired token.");
            }

            var courses = await _teacherService.GetCoursesAsync(teacherId);

            GradeLogger.Instance.LogMessage("Courses retrieved successfully.");
            return Ok(courses);
        }

        // GET api/Teacher/courses
        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses()
        {
            if (!ValidateTeacherToken(out int teacherId))
            {
                GradeLogger.Instance.LogError($"Invalid or expired token received for GetCourses. Teacher id: {teacherId}");
                return Unauthorized("Invalid or expired token.");
            }

            var courses = await _teacherService.GetAllCoursesAsync();

            GradeLogger.Instance.LogMessage("Courses retrieved successfully.");
            return Ok(courses);
        }

        // GET api/Teacher/classes/{classId}/students
        [HttpGet("classes/{classId}/students")]
        public async Task<IActionResult> GetStudents(int classId)
        {
            if (!ValidateTeacherToken(out int teacherId))
            {
                GradeLogger.Instance.LogError($"Invalid or expired token received for GetStudents. Teacher id: {teacherId}");
                return Unauthorized("Invalid or expired token.");
            }

            var students = await _teacherService.GetStudentsInClassAsync(teacherId, classId);
            GradeLogger.Instance.LogMessage("Students retrieved successfully.");
            return Ok(students);
        }

        // Helper method to validate token and extract teacherId
        private bool ValidateTeacherToken(out int teacherId)
        {
            teacherId = 0;
            var user = HttpContext.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                return false;

            // Check role
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.Equals(roleClaim, TEACHER_ROLE, StringComparison.OrdinalIgnoreCase))
                return false;

            // Check expiration
            var expClaim = user.FindFirst("exp")?.Value;
            if (expClaim != null && long.TryParse(expClaim, out long expSeconds))
            {
                var expTime = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
                if (expTime < DateTime.UtcNow)
                    return false;
            }
            else
            {
                return false;
            }

            // Check issuer
            var issuerClaim = user.FindFirst("iss")?.Value;
            var expectedIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            if (!string.Equals(issuerClaim, expectedIssuer, StringComparison.Ordinal))
                return false;

            // Get teacherId from NameIdentifier claim
            var teacherIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (teacherIdClaim == null || !int.TryParse(teacherIdClaim, out teacherId))
                return false;

            return true;
        }
    }
}
