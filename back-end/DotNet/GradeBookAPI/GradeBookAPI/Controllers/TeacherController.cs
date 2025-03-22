using GradeBookAPI.DTOs.DataDTOs;
using GradeBookAPI.Entities;
using GradeBookAPI.Helpers;
using GradeBookAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

using GradeLogger = GradeBookAPI.Logger.Logger;

namespace GradeBookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // We need authentication for all endpoints in this controller
    public class TeacherController(ITeacherService teacherService) : ControllerBase
    {
        private readonly ITeacherService _teacherService = teacherService;

        // POST api/Teacher/classes/{classId}/students
        [HttpPost("classes/{classId}/students")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddStudent(int classId, [FromBody] AddStudentRequest request)
        {
            if (!ModelState.IsValid)
            {
                var auditLog = new AuditLog
                {
                    UserId = 1,
                    Action = "AddStudent",
                    EntityType = "Teacher",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = "Invalid input data received for AddStudent" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
            }

            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "AddStudent",
                    EntityType = "Teacher",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = "Invalid or expired token", teacherId }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return Unauthorized("Invalid or expired token.");
            }

            var result = await _teacherService.AddStudentToClassAsync(teacherId, classId, request);
            if (!result)
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "AddStudent",
                    EntityType = "Teacher",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = "Unable to add student", studentId = request.StudentId }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return BadRequest("Unable to add student.");
            }

            var successLog = new AuditLog
            {
                UserId = teacherId,
                Action = "AddStudent",
                EntityType = "Teacher",
                EntityId = classId,
                Details = JsonSerializer.Serialize(new { message = "Student added successfully", studentId = request.StudentId, classId = classId }),
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                CreatedAt = DateTime.UtcNow
            };
            GradeLogger.Instance.LogMessage(successLog);
            return Ok("Student added successfully.");
        }

        // DELETE api/Teacher/classes/{classId}/students/{studentId}
        [HttpDelete("classes/{classId}/students/{studentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveStudent(int classId, int studentId)
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "RemoveStudent",
                    EntityType = "Teacher",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = "Invalid or expired token", teacherId }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return Unauthorized("Invalid or expired token.");
            }

            var result = await _teacherService.RemoveStudentFromClassAsync(teacherId, classId, studentId);
            if (!result)
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "RemoveStudent",
                    EntityType = "Teacher",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = "Unable to remove student", studentId, classId }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return BadRequest("Unable to remove student.");
            }

            var successLog = new AuditLog
            {
                UserId = teacherId,
                Action = "RemoveStudent",
                EntityType = "Teacher",
                EntityId = classId,
                Details = JsonSerializer.Serialize(new { message = "Student removed successfully", studentId = studentId, classId = classId }),
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                CreatedAt = DateTime.UtcNow
            };
            GradeLogger.Instance.LogMessage(successLog);
            return Ok("Student removed successfully.");
        }

        // POST api/Teacher/classes/{classId}/students/batch
        [HttpPost("classes/{classId}/students/batch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddStudents(int classId, [FromBody] AddStudentsRequest request)
        {
            if (!ModelState.IsValid)
            {
                var auditLog = new AuditLog
                {
                    UserId = 1,
                    Action = "AddStudents",
                    EntityType = "Teacher",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = "Invalid input data received for AddStudents" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
            }

            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "AddStudents",
                    EntityType = "Teacher",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = "Invalid or expired token", teacherId }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return Unauthorized("Invalid or expired token.");
            }

            var result = await _teacherService.AddStudentsToClassAsync(teacherId, classId, request);
            if (!result)
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "AddStudents",
                    EntityType = "Teacher",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = "Unable to add some or all students" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return BadRequest("Unable to add some or all students.");
            }

            var successLog = new AuditLog
            {
                UserId = teacherId,
                Action = "AddStudents",
                EntityType = "Teacher",
                EntityId = classId,
                Details = JsonSerializer.Serialize(new { message = "Students added successfully" }),
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                CreatedAt = DateTime.UtcNow
            };
            GradeLogger.Instance.LogMessage(successLog);
            return Ok("Students added successfully.");
        }

        // DELETE api/Teacher/classes/{classId}/students/batch
        [HttpDelete("classes/{classId}/students/batch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveStudents(int classId, [FromBody] RemoveStudentsRequest request)
        {
            if (!ModelState.IsValid)
            {
                var auditLog = new AuditLog
                {
                    UserId = 1,
                    Action = "RemoveStudents",
                    EntityType = "Teacher",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = "Invalid input data received for RemoveStudents" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
            }

            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "RemoveStudents",
                    EntityType = "Teacher",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = "Invalid or expired token", teacherId }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return Unauthorized("Invalid or expired token.");
            }

            var result = await _teacherService.RemoveStudentsFromClassAsync(teacherId, classId, request);
            if (!result)
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "RemoveStudents",
                    EntityType = "Teacher",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = "Unable to remove some or all students" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return BadRequest("Unable to remove some or all students.");
            }

            var successLog = new AuditLog
            {
                UserId = teacherId,
                Action = "RemoveStudents",
                EntityType = "Teacher",
                EntityId = classId,
                Details = JsonSerializer.Serialize(new { message = "Students removed successfully" }),
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                CreatedAt = DateTime.UtcNow
            };
            GradeLogger.Instance.LogMessage(successLog);
            return Ok("Students removed successfully.");
        }

        // POST api/Teacher/courses
        [HttpPost("courses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
        {
            if (!ModelState.IsValid)
            {
                var auditLog = new AuditLog
                {
                    UserId = 1,
                    Action = "CreateCourse",
                    EntityType = "Teacher",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Invalid input data received for CreateCourse" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
            }

            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "CreateCourse",
                    EntityType = "Teacher",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Invalid or expired token", teacherId }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return Unauthorized("Invalid or expired token.");
            }
            try
            {
                var result = await _teacherService.CreateCourseAsync(teacherId, request);

                if (!result)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = teacherId,
                        Action = "CreateCourse",
                        EntityType = "Teacher",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "Course creation failed" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);
                    return BadRequest("Course creation failed.");
                }
            }
            catch (Exception ex)
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "CreateCourse",
                    EntityType = "Teacher",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Course creation failed", exception = ex.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return BadRequest("Course creation failed.");
            }


            var successLog = new AuditLog
            {
                UserId = teacherId,
                Action = "CreateCourse",
                EntityType = "Teacher",
                EntityId = 0,
                Details = JsonSerializer.Serialize(new { message = "Course created successfully" }),
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                CreatedAt = DateTime.UtcNow
            };
            GradeLogger.Instance.LogMessage(successLog);
            return Ok("Course created successfully.");
        }

        // POST api/Teacher/classes
        [HttpPost("classes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassRequest request)
        {
            if (!ModelState.IsValid)
            {
                var auditLog = new AuditLog
                {
                    UserId = 1,
                    Action = "CreateClass",
                    EntityType = "Teacher",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Invalid input data received for CreateClass" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return BadRequest(new { Success = false, Message = "Invalid input data", Errors = ModelState });
            }
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "CreateClass",
                    EntityType = "Teacher",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Invalid or expired token", teacherId }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return Unauthorized("Invalid or expired token.");
            }

            try
            {
                var result = await _teacherService.CreateClassAsync(teacherId, request);

                if (!result)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = teacherId,
                        Action = "CreateClass",
                        EntityType = "Teacher",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "Class creation failed. Verify that the course exists" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);
                    return BadRequest("Class creation failed. Verify that the course exists.");
                }
            }
            catch (Exception ex)
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "CreateClass",
                    EntityType = "Teacher",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Class creation failed", exception = ex.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return BadRequest("Class creation failed.");
            }


            var successLog = new AuditLog
            {
                UserId = teacherId,
                Action = "CreateClass",
                EntityType = "Teacher",
                EntityId = 0,
                Details = JsonSerializer.Serialize(new { message = "Class created successfully" }),
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                CreatedAt = DateTime.UtcNow
            };
            GradeLogger.Instance.LogMessage(successLog);
            return Ok("Class created successfully.");
        }

        // GET api/Teacher/classes
        [HttpGet("classes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetClasses()
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "GetClasses",
                    EntityType = "Teacher",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Invalid or expired token", teacherId }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return Unauthorized("Invalid or expired token.");
            }

            var classes = await _teacherService.GetClassesAsync(teacherId);

            var successLog = new AuditLog
            {
                UserId = teacherId,
                Action = "GetClasses",
                EntityType = "Teacher",
                EntityId = 0,
                Details = JsonSerializer.Serialize(new { message = "Classes retrieved successfully" }),
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                CreatedAt = DateTime.UtcNow
            };
            GradeLogger.Instance.LogMessage(successLog);
            return Ok(classes);
        }

        // GET api/Teacher/courses/assigned
        [HttpGet("courses/assigned")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAssignedCourses()
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "GetAssignedCourses",
                    EntityType = "Teacher",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Invalid or expired token", teacherId }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return Unauthorized("Invalid or expired token.");
            }

            var courses = await _teacherService.GetCoursesAsync(teacherId);

            var successLog = new AuditLog
            {
                UserId = teacherId,
                Action = "GetAssignedCourses",
                EntityType = "Teacher",
                EntityId = 0,
                Details = JsonSerializer.Serialize(new { message = "Courses retrieved successfully" }),
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                CreatedAt = DateTime.UtcNow
            };
            GradeLogger.Instance.LogMessage(successLog);
            return Ok(courses);
        }

        // GET api/Teacher/courses
        [HttpGet("courses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCourses()
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "GetCourses",
                    EntityType = "Teacher",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Invalid or expired token", teacherId }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return Unauthorized("Invalid or expired token.");
            }

            var courses = await _teacherService.GetAllCoursesAsync();

            var successLog = new AuditLog
            {
                UserId = teacherId,
                Action = "GetCourses",
                EntityType = "Teacher",
                EntityId = 0,
                Details = JsonSerializer.Serialize(new { message = "Courses retrieved successfully" }),
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                CreatedAt = DateTime.UtcNow
            };
            GradeLogger.Instance.LogMessage(successLog);
            return Ok(courses);
        }

        // GET api/Teacher/classes/{classId}/students
        [HttpGet("classes/{classId}/students")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStudents(int classId)
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "GetStudents",
                    EntityType = "Teacher",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = "Invalid or expired token", teacherId }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return Unauthorized("Invalid or expired token.");
            }

            var students = await _teacherService.GetStudentsInClassAsync(teacherId, classId);
            var successLog = new AuditLog
            {
                UserId = teacherId,
                Action = "GetStudents",
                EntityType = "Teacher",
                EntityId = classId,
                Details = JsonSerializer.Serialize(new { message = "Students retrieved successfully" }),
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                CreatedAt = DateTime.UtcNow
            };
            GradeLogger.Instance.LogMessage(successLog);
            return Ok(students);
        }
    }
}
